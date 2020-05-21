<template>
    <div class="code-line" :class="{ 'highlight': highlight }">
        <span v-text="line">
        </span>
    </div>
</template>

<script>
export default {
    props: ['line', 'lineNumber', 'highlight'],
}
</script>

<style scoped>
.code-line{
    white-space: pre;
    padding-top: 0;
}

.highlight{
    background: #222;
    color: lightcoral;
}
</style>