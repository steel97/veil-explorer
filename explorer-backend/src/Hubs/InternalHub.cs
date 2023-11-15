using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using ExplorerBackend.Models.System;
using ExplorerBackend.Configs;
using ExplorerBackend.Services.Core;
using ExplorerBackend.Services.Caching;
using ExplorerBackend.Services.Data;
using NanoXLSX;

namespace ExplorerBackend.Hubs;

public class InternalHub(ILogger<EventsHub> logger, ChaininfoSingleton chainInfoSingleton, InternalSingleton internalSingleton, IOptions<ServerConfig> serverConfig,
    ITransactionsDataService transactionsDataService, ITransactionDecoder transactionDecoder, IUtilityService utilityService)
    : Hub
{
    private readonly ChaininfoSingleton _chainInfoSingleton = chainInfoSingleton;
    private readonly InternalSingleton _internalSingleton = internalSingleton;
    private readonly ILogger _logger = logger;
    private readonly IOptions<ServerConfig> _serverConfig = serverConfig;
    private readonly ITransactionsDataService _transactionsDataService = transactionsDataService; // switched to the new layer
    private readonly ITransactionDecoder _transactionDecoder = transactionDecoder;
    private readonly IUtilityService _utilityService = utilityService;

    public async Task FetchBasecoinTxs(string accessKey, string[] addresses)//, CancellationToken cancellationToken = default)
    {
        var cancellationToken = CancellationToken.None;
        if (accessKey != _serverConfig.Value.InternalAccessKey)
        {
            await Clients.Caller.SendAsync("error", "Error: Wrong access key", cancellationToken);
            return;
        }

        foreach (var address in addresses)
        {
            if (!_utilityService.VerifyAddress(address))
            {
                await Clients.Caller.SendAsync("error", "Error: invalid address: " + address, cancellationToken);
                return;
            }
        }

        if (addresses.Length == 0)
        {
            await Clients.Caller.SendAsync("error", "Error: please specify at least 1 address", cancellationToken);
            return;
        }

        try
        {
            _internalSingleton.ActiveExportTasks++;

            var internalId = DateTimeOffset.Now.ToUnixTimeSeconds();

            await Clients.Caller.SendAsync("dllink", internalId, cancellationToken); // end

            var txInputs = new Dictionary<string, Dictionary<string, double>>(); // address, Dict<txid, amount>
            var txOutputs = new Dictionary<string, Dictionary<string, double>>(); // address, Dict<txid, amount>

            var mBlock = _chainInfoSingleton.CurrentSyncedBlock;
            var si = 0;
            for (var i = 1; i <= mBlock; i++)
            {
                var rtxs = await _transactionsDataService.GetTransactionsForBlockAsync(i, 0, 0, true, cancellationToken);
                if (rtxs != null)
                {
                    var txTargets = new List<TxDecodeTarget>();

                    rtxs.ForEach(rtx => txTargets.Add(new TxDecodeTarget
                    {
                        TxId = rtx.txid_hex!,
                        Data = rtx.data
                    }));

                    var decodedTransactions = await _transactionDecoder.DecodeTransactionsAsync(txTargets, i, cancellationToken);

                    if (decodedTransactions != null)
                        foreach (var dtr in decodedTransactions)
                        {
                            /* search for address in inputs (send operations) */
                            if (dtr.Inputs != null)
                                foreach (var input in dtr.Inputs)
                                {
                                    input.PrevOutAddresses?.ForEach(addr =>
                                    {
                                        foreach (var address in addresses)
                                        {

                                            if (addr == address)
                                            {
                                                if (!txInputs.ContainsKey(address))
                                                    txInputs.Add(address, new());

                                                if (txInputs[address].ContainsKey(dtr.TxId!))
                                                    txInputs[address][dtr.TxId!] += input.PrevOutAmount / 100000000.0d;
                                                else
                                                    txInputs[address].Add(dtr.TxId!, input.PrevOutAmount / 100000000.0d);
                                            }

                                        }
                                    });
                                }
                            /* search for address in outputs (receive operations) */
                            if (dtr.Outputs != null)
                                foreach (var output in dtr.Outputs)
                                {
                                    output.Addresses?.ForEach(addr =>
                                    {
                                        foreach (var address in addresses)
                                        {

                                            if (addr == address)
                                            {
                                                if (!txOutputs.ContainsKey(address))
                                                    txOutputs.Add(address, new());

                                                if (txOutputs[address].ContainsKey(dtr.TxId!))
                                                    txOutputs[address][dtr.TxId!] += output.Amount / 100000000.0d;
                                                else
                                                    txOutputs[address].Add(dtr.TxId!, output.Amount / 100000000.0d);
                                            }

                                        }
                                    });
                                }
                        }
                }

                if (si >= 1000)
                {
                    si = 0;
                    try
                    {
                        await Clients.Caller.SendAsync("progress", i, mBlock, cancellationToken); // progress
                    }
                    catch
                    {

                    }
                }
                si++;
            }

            var fileId = "./data/export-txs-" + internalId + ".xlsx";

            var workbook = new Workbook(fileId, "Addr-1");
            if (addresses.Length > 1)
                for (var i = 2; i <= addresses.Length; i++)
                    workbook.AddWorksheet("Addr-" + i);

            var index = 1;
            foreach (var address in addresses)
            {
                try
                {
                    workbook.SetCurrentWorksheet("Addr-" + index);
                    workbook.CurrentWorksheet.AddNextCell("Address");
                    workbook.CurrentWorksheet.AddNextCell(address);
                    workbook.CurrentWorksheet.GoToNextRow();

                    workbook.CurrentWorksheet.AddNextCell("Sent");
                    workbook.CurrentWorksheet.AddNextCell("");
                    workbook.CurrentWorksheet.AddNextCell("Received");
                    workbook.CurrentWorksheet.AddNextCell("");
                    workbook.CurrentWorksheet.GoToNextRow();

                    workbook.CurrentWorksheet.AddNextCell("Tx ID");
                    workbook.CurrentWorksheet.AddNextCell("Amount");
                    workbook.CurrentWorksheet.AddNextCell("Tx ID");
                    workbook.CurrentWorksheet.AddNextCell("Amount");

                    if (txInputs.TryGetValue(address, out var value))
                    {
                        var fieldIndex = 0;
                        foreach (var input in value)
                        {
                            workbook.CurrentWorksheet.AddCell(input.Key, 0, 3 + fieldIndex); // input tx
                            workbook.CurrentWorksheet.AddCell(input.Value, 1, 3 + fieldIndex); // input amount

                            fieldIndex++;
                        }
                    }

                    if (txOutputs.TryGetValue(address, out var value2))
                    {
                        var fieldIndex = 0;
                        foreach (var output in value2)
                        {
                            workbook.CurrentWorksheet.AddCell(output.Key, 2, 3 + fieldIndex); // output tx
                            workbook.CurrentWorksheet.AddCell(output.Value, 3, 3 + fieldIndex); // output amount

                            fieldIndex++;
                        }
                    }
                }
                catch
                {

                }

                index++;
            }

            workbook.Save();
            /*try
            {
                await Clients.Caller.SendAsync("done", internalId, cancellationToken); // end
            }
            catch
            {

            }*/
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured at InternalHub/FetchBasecoinTxs");
        }
        finally
        {
            _internalSingleton.ActiveExportTasks--;
        }
    }
}