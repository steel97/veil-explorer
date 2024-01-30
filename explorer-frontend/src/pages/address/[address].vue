<template>
  <div>
    <h1 class="font-semibold py-4">
      <span class="uppercase">{{ t("Address.ViewTitle") }}:</span>
      &nbsp;
      <button class="inline-flex items-center max-w-full" @click="copyToClipboard()"
        :title="t('Address.CopyToClipboard')">
        <span class="
            text-sky-700
            dark:text-sky-400
            overflow-hidden
            text-ellipsis
            max-address-width
          ">{{ address }}</span>&nbsp;
        <DocumentDuplicateIcon class="
            inline-block
            h-5
            w-5
            mr-2
            text-sky-700
            dark:text-sky-400
            cursor-pointer
          " />
      </button>
    </h1>
    <div class="rounded p-4 bg-gray-50 dark:bg-gray-800 text-sm">
      <div class="md:grid grid-cols-2">
        <div>
          <!-- Info section -->
          <div class="grid grid-cols-2 gap-0.5 w-full py-4 border-b" v-for="(val, index) in getAddressDetails"
            :class="index < getAddressDetails.length - 1 ? '' : 'md:border-b-0'" :key="'detail-' + index">
            <div>{{ t(val.placeholder) }}</div>
            <div class="text-right md:text-left overflow-hidden text-ellipsis">
              <span v-if="val.check == null">{{ val.value }}</span>
              <span v-else :title="val.check ? t('Address.Yes') : t('Address.No')">
                <CheckIcon v-if="val.check" class="
                    inline-block
                    h-5
                    w-5
                    mr-2
                    text-sky-700
                    dark:text-sky-400
                  " />
                <XMarkIcon v-if="!val.check" class="
                    inline-block
                    h-5
                    w-5
                    mr-2
                    text-sky-700
                    dark:text-sky-400
                  " />
              </span>
            </div>
          </div>
        </div>
        <div class="flex justify-start md:justify-end mt-4 md:mt-0">
          <!-- QR section -->
          <div>
            <!-- TO-DO, use <client-only/> when nuxt3 implement it -->
            <QrcodeVue v-if="renderQR" :value="addressReactive" :size="180" :margin="2" render-as="svg" level="H"
              :aria-label="t('Address.QrCode')" />
            <div>
              <div class="flex justify-between mt-2">
                <div>{{ t("Address.Balance") }}:</div>
                <a href="javascript:void(0)" class="
                    text-sky-700
                    dark:text-sky-400
                    hover:underline
                    underline-offset-4
                  " @click="reloadBalance()" v-if="addressInfo != null &&
                    addressInfo.amountFetched &&
                    !reloadingBalance &&
                    addressInfo.address != null &&
                    (addressInfo.address.isstealthaddress == null ||
                      !addressInfo.address.isstealthaddress)
                    ">{{ t("Address.Update") }}</a>
              </div>
              <div v-if="!reloadingBalance &&
                addressInfo != null &&
                addressInfo.amountFetched
                " class="mt-1">
                {{ addressInfo.amount }} veil
              </div>
              <div v-else-if="!reloadingBalance &&
                addressInfo != null &&
                addressInfo.address != null &&
                addressInfo.address.isstealthaddress != null &&
                addressInfo.address.isstealthaddress
                " class="mt-1">
                {{ t("Address.BalanceHidden") }}
              </div>
              <div v-else class="flex items-center mt-1">
                <svg class="
                    animate-spin
                    ml-1
                    mr-3
                    h-5
                    w-5
                    text-gray-800
                    dark:text-gray-300
                  " xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z">
                  </path>
                </svg>
                {{ t("Address.Loading") }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AddressResponse } from "@/models/API/AddressResponse";
import DocumentDuplicateIcon from "@heroicons/vue/24/solid/DocumentDuplicateIcon";
import CheckIcon from "@heroicons/vue/24/solid/CheckIcon";
import XMarkIcon from "@heroicons/vue/24/solid/XMarkIcon";
import QrcodeVue from "qrcode.vue";
import Toastify from "toastify-js";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const route = useRoute();
const config = useRuntimeConfig();

const address: string = route.params.address as string;
const addressReactive = ref(address);
const reloadingBalance = ref(false);
const renderQR = ref(false);
const router = useRouter();

const getAddressDetails = computed(() => {
  if (addressInfo.value == null) return [];

  const res = [];

  if (addressInfo.value.hash != null)
    res.push({
      placeholder: "Address.Hash160",
      value: addressInfo.value.hash,
      check: null,
    });

  if (addressInfo.value.version != null)
    res.push({
      placeholder: "Address.Version",
      value: addressInfo.value.version,
      check: null,
    });

  if (addressInfo.value.scriptHash != null)
    res.push({
      placeholder: "Address.Scripthash",
      value: addressInfo.value.scriptHash,
      check: null,
    });

  if (addressInfo.value.address != null) {
    if (addressInfo.value.address.scriptPubKey != null)
      res.push({
        placeholder: "Address.ScriptPublicKey",
        value: addressInfo.value.address.scriptPubKey,
        check: null,
      });

    if (addressInfo.value.address.isvalid != null)
      res.push({
        placeholder: "Address.IsValid",
        value: addressInfo.value.address.isvalid,
        check: addressInfo.value.address.isvalid,
      });

    if (addressInfo.value.address.isstealthaddress != null)
      res.push({
        placeholder: "Address.IsStealth",
        value: addressInfo.value.address.isstealthaddress,
        check: addressInfo.value.address.isstealthaddress,
      });

    if (addressInfo.value.address.iswitness != null)
      res.push({
        placeholder: "Address.IsWitness",
        value: addressInfo.value.address.iswitness,
        check: addressInfo.value.address.iswitness,
      });

    if (addressInfo.value.address.witness_version != null)
      res.push({
        placeholder: "Address.WitnessVersion",
        value: addressInfo.value.address.witness_version,
        check: null,
      });

    if (addressInfo.value.address.witness_program != null)
      res.push({
        placeholder: "Address.WitnessProgram",
        value: addressInfo.value.address.witness_program,
        check: null,
      });
  }

  return res;
});

const copyToClipboard = () => {
  try {
    navigator.clipboard.writeText(address).then(
      () => {
        Toastify({
          text: t("Address.CopySuccess"),
          duration: 2000,
          close: false,
          gravity: "top",
          position: "right",
          className: "",
          stopOnFocus: true,
        }).showToast();
      },
      () => {
        console.log("Can't copy text to clipboard (1)");
      }
    );
  } catch {
    console.log("Can't copy text to clipboard (2)");
  }
};

const fetchAddress = async (forceScanAmount = false, isInitial = false) => {
  try {
    if (isInitial) {
      const { data } = await useFetch<AddressResponse>(
        `${getApiPath()}/address`,
        {
          method: "POST",
          body: {
            address: address,
            forceScanAmount: forceScanAmount,
          },
        }
      );
      return data.value;
    } else {
      const data = await $fetch<AddressResponse>(
        `${getApiPath()}/address`,
        {
          method: "POST",
          body: {
            address: address,
            forceScanAmount: forceScanAmount,
          },
        }
      );
      return data;
    }
  } catch {
    return null;
  }
};

const reloadBalance = async () => {
  reloadingBalance.value = true;
  addressInfo.value = await fetchAddress(true);
  reloadingBalance.value = false;
};

const addressInfo = ref(await fetchAddress(false, true));

const shouldFetchBalance = () =>
  addressInfo.value != null &&
  addressInfo.value.address != null &&
  (addressInfo.value.address.isstealthaddress == null ||
    !addressInfo.value.address.isstealthaddress) &&
  !addressInfo.value.amountFetched;

const checkLoad = async () => {
  if (shouldFetchBalance()) {
    addressInfo.value = await fetchAddress();
    setTimeout(checkLoad, 500);
  }
};

if (addressInfo.value != null && !addressInfo.value.fetched)
  router.push("/search/notfound");

onMounted(() => {
  if (process.client) renderQR.value = true;
  setTimeout(checkLoad, 500);
});

const meta = computed(() => {
  return {
    title: t("Address.Meta.Title", { address: address }),
    meta: [
      {
        name: "description",
        content: t("Address.Meta.Description", { address: address }),
      },
      {
        name: "og:title",
        content: t("Address.Meta.Title", { address: address }),
      },
      {
        name: "og:url",
        content: `${config.public.baseUrl}/address/${address}`,
      },
    ],
  };
});
useHead(meta);
</script>

<style scoped>
.max-address-width {
  max-width: calc(100% - 30px);
}
</style>