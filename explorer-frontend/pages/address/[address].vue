<template>
  <div>
    <h1 class="font-semibold py-4">
      <span class="uppercase">{{ t("Address.ViewTitle") }}:</span>
      &nbsp;
      <button
        class="inline-flex items-center max-w-full"
        @click="copyToClipboard()"
        :title="t('Address.CopyToClipboard')"
      >
        <span
          class="
            text-sky-700
            dark:text-sky-400
            overflow-hidden
            text-ellipsis
            max-address-width
          "
          >{{ address }}</span
        >&nbsp;
        <DuplicateIcon
          class="
            inline-block
            h-5
            w-5
            mr-2
            text-sky-700
            dark:text-sky-400
            cursor-pointer
          "
        />
      </button>
    </h1>
    <div class="rounded p-4 bg-gray-50 dark:bg-gray-800 text-sm">
      <div class="md:grid grid-cols-2">
        <div>
          <!-- Info section -->
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="addressInfo != null && addressInfo.hash != null"
          >
            <div>{{ t("Address.Hash160") }}</div>
            <div class="text-right md:text-left overflow-hidden text-ellipsis">
              {{ addressInfo.hash }}
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="
              addressInfo != null &&
              addressInfo.address != null &&
              addressInfo.address.scriptPubKey != null &&
              addressInfo.address.scriptPubKey != ''
            "
          >
            <div>{{ t("Address.ScriptPublicKey") }}</div>
            <div class="text-right md:text-left overflow-hidden text-ellipsis">
              {{ addressInfo.address.scriptPubKey }}
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="addressInfo != null && addressInfo.version != null"
          >
            <div>{{ t("Address.Version") }}</div>
            <div class="text-right md:text-left">{{ addressInfo.version }}</div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="addressInfo != null && addressInfo.scriptHash"
          >
            <div>{{ t("Address.Scripthash") }}</div>
            <div class="text-right md:text-left overflow-hidden text-ellipsis">
              {{ addressInfo.scriptHash }}
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="addressInfo != null && addressInfo.address != null"
          >
            <div>{{ t("Address.IsValid") }}</div>
            <div class="text-right md:text-left">
              <CheckIcon
                v-if="addressInfo.address.isvalid"
                :title="t('Address.Yes')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
              <XIcon
                v-if="!addressInfo.address.isvalid"
                :title="t('Address.No')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="
              addressInfo != null &&
              addressInfo.address != null &&
              addressInfo.address.isstealthaddress
            "
          >
            <div>{{ t("Address.IsStealth") }}</div>
            <div class="text-right md:text-left">
              <CheckIcon
                v-if="addressInfo.address.isstealthaddress"
                :title="t('Address.Yes')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
              <XIcon
                v-if="!addressInfo.address.isstealthaddress"
                :title="t('Address.No')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="addressInfo != null && addressInfo.address != null"
          >
            <div>{{ t("Address.IsScript") }}</div>
            <div class="text-right md:text-left">
              <CheckIcon
                v-if="addressInfo.address.isscript"
                :title="t('Address.Yes')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
              <XIcon
                v-if="!addressInfo.address.isscript"
                :title="t('Address.No')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="addressInfo != null && addressInfo.address != null"
          >
            <div>{{ t("Address.IsWitness") }}</div>
            <div class="text-right md:text-left">
              <CheckIcon
                v-if="addressInfo.address.iswitness"
                :title="t('Address.Yes')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
              <XIcon
                v-if="!addressInfo.address.iswitness"
                :title="t('Address.No')"
                class="inline-block h-5 w-5 mr-2 text-sky-700 dark:text-sky-400"
              />
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="
              addressInfo != null &&
              addressInfo.address != null &&
              addressInfo.address.witness_version
            "
          >
            <div>{{ t("Address.WitnessVersion") }}</div>
            <div class="text-right md:text-left">
              {{ addressInfo.address.witness_version }}
            </div>
          </div>
          <div
            class="grid grid-cols-2 gap-0.5 mb-2 w-full"
            v-if="
              addressInfo != null &&
              addressInfo.address != null &&
              addressInfo.address.witness_program
            "
          >
            <div>{{ t("Address.WitnessProgram") }}</div>
            <div class="text-right md:text-left overflow-hidden text-ellipsis">
              {{ addressInfo.address.witness_program }}
            </div>
          </div>
        </div>
        <div class="flex justify-start md:justify-end">
          <!-- QR section -->
          <div>
            <QrcodeVue
              :value="addressReactive"
              :size="180"
              :margin="2"
              render-as="svg"
              level="H"
            />
            <div>
              <div class="flex justify-between mt-2">
                <div>{{ t("Address.Balance") }}:</div>
                <a
                  href="javascript:void(0)"
                  class="
                    text-sky-700
                    dark:text-sky-400
                    hover:underline
                    underline-offset-4
                  "
                  @click="reloadBalance()"
                  v-if="
                    addressInfo != null &&
                    addressInfo.amountFetched &&
                    !reloadingBalance &&
                    addressInfo.address != null &&
                    (addressInfo.address.isstealthaddress == null ||
                      !addressInfo.address.isstealthaddress)
                  "
                  >{{ t("Address.Update") }}</a
                >
              </div>
              <div
                v-if="
                  !reloadingBalance &&
                  addressInfo != null &&
                  addressInfo.amountFetched
                "
                class="mt-1"
              >
                {{ addressInfo.amount }} veil
              </div>
              <div
                v-else-if="
                  !reloadingBalance &&
                  addressInfo != null &&
                  addressInfo.address != null &&
                  addressInfo.address.isstealthaddress != null &&
                  addressInfo.address.isstealthaddress
                "
                class="mt-1"
              >
                {{ t("Address.BalanceHidden") }}
              </div>
              <div v-else class="flex items-center mt-1">
                <svg
                  class="
                    animate-spin
                    ml-1
                    mr-3
                    h-5
                    w-5
                    text-gray-800
                    dark:text-gray-300
                  "
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                >
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                  ></circle>
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  ></path>
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
import { AddressResponse } from "@/models/API/AddressResponse";
import { DuplicateIcon, CheckIcon, XIcon } from "@heroicons/vue/solid";
import { useI18n } from "vue-i18n";
import QrcodeVue from "qrcode.vue";
import Toastify from "toastify-js";

const { t } = useI18n();
const { getApiPath } = useConfigs();
const route = useRoute();
const config = useRuntimeConfig();

const address: string = route.params.address;
const addressReactive = ref(address);
const reloadingBalance = ref(false);

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

const fetchAddress = async (forceScanAmount = false) => {
  try {
    const { data } = await useFetch<string, AddressResponse>(
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
  } catch {
    return null;
  }
};

const reloadBalance = async () => {
  reloadingBalance.value = true;
  addressInfo.value = await fetchAddress(true);
  reloadingBalance.value = false;
};

const addressInfo = ref(await fetchAddress());

const shouldFetchBalance = () =>
  addressInfo.value != null &&
  addressInfo.value.address != null &&
  (addressInfo.value.address.isstealthaddress == null ||
    !addressInfo.value.address.isstealthaddress) &&
  !addressInfo.value.amountFetched;

const checkBalance = async () => {
  if (shouldFetchBalance()) {
    addressInfo.value = await fetchAddress();
    setTimeout(checkBalance, 500);
  }
};

onMounted(() => {
  setTimeout(checkBalance, 500);
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
        content: `${config.BASE_URL}/`,
      },
    ],
  };
});
useMeta(meta);
</script>

<style scoped>
.max-address-width {
  max-width: calc(100% - 30px);
}
</style>