<template>
  <div class="flex justify-center flex-wrap mt-4" v-show="localMaxPages > 1">
    <div v-for="(element, index) in elements" :key="'pagination-' + index">
      <a
        v-if="element.interactable"
        :href="element.link"
        @click.prevent="navigateTo(element.targetPage)"
        :class="
          element.current
            ? 'bg-sky-700 text-gray-50 dark:bg-sky-700 dark:text-gray-50'
            : ''
        "
        class="
          hover:bg-sky-700
          hover:dark:bg-sky-700
          hover:text-gray-50
          hover:dark:text-gray-50
          p-2
          smargins
          bg-gray-50
          dark:bg-gray-800
          pagi
          h-auto
          flex
          justify-center
          items-center
          text-sm
        "
      >
        {{ element.text }}
      </a>
      <button
        class="
          p-2
          smargins
          bg-gray-300
          dark:bg-gray-600
          pagi
          h-auto
          flex
          justify-center
          items-center
          text-sm
          cursor-not-allowed
        "
        disabled
        v-if="!element.interactable"
      >
        {{ element.text }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = defineProps<{
  overallEntries: number;
  entriesPerPage: number;
  currentPage: number;
  linkTemplate: string;
}>();

const emit = defineEmits<{
  (e: "pageSelected", value: number): void;
}>();

const localMaxPages = ref(0);

interface NavElement {
  interactable: boolean;
  current: boolean;
  text: string;
  link: string;
  targetPage: number;
}

const pmOffset = 4;

const navigateTo = (targetPage: number) => emit("pageSelected", targetPage);

const elements = computed(() => {
  const targetElements: Array<NavElement> = [];
  targetElements.push({
    interactable: props.currentPage > 1,
    current: props.currentPage == 1,
    text: "«",
    link: props.linkTemplate.replace(
      "{page}",
      (props.currentPage - 1).toString()
    ),
    targetPage: props.currentPage - 1,
  });

  targetElements.push({
    interactable: props.currentPage > 1,
    current: props.currentPage == 1,
    text: "1",
    link: props.linkTemplate.replace("{page}", "1"),
    targetPage: 1,
  });

  if (props.currentPage - pmOffset > 1)
    targetElements.push({
      interactable: false,
      current: false,
      text: "...",
      link: props.linkTemplate.replace("{page}", "1"),
      targetPage: 1,
    });

  const maxPages = Math.ceil(props.overallEntries / props.entriesPerPage);
  localMaxPages.value = maxPages;

  let renderedEntriesBefore = 0;
  let renderedEntriesAfter = 0;

  for (
    let i = props.currentPage - pmOffset;
    i <= props.currentPage + pmOffset;
    i++
  ) {
    if (i >= maxPages) break;
    if (i > 1) {
      targetElements.push({
        interactable: true,
        current: i == props.currentPage,
        text: i.toString(),
        link: props.linkTemplate.replace("{page}", i.toString()),
        targetPage: i,
      });
      if (i < props.currentPage) renderedEntriesBefore++;
      if (i > props.currentPage) renderedEntriesAfter++;
      if (renderedEntriesBefore >= pmOffset && renderedEntriesAfter >= pmOffset)
        break;
    }
  }

  if (props.currentPage + pmOffset < maxPages)
    targetElements.push({
      interactable: false,
      current: false,
      text: "...",
      link: props.linkTemplate.replace("{page}", maxPages.toString()),
      targetPage: maxPages,
    });

  targetElements.push({
    interactable: props.currentPage < maxPages,
    current: props.currentPage == maxPages,
    text: maxPages.toString(),
    link: props.linkTemplate.replace("{page}", maxPages.toString()),
    targetPage: maxPages,
  });

  targetElements.push({
    interactable: props.currentPage < maxPages,
    current: false,
    text: "»",
    link: props.linkTemplate.replace(
      "{page}",
      (props.currentPage + 1).toString()
    ),
    targetPage: props.currentPage + 1,
  });

  return targetElements;
});
</script>

<style scoped>
.pagi {
  min-width: 2.5rem;
}
</style>