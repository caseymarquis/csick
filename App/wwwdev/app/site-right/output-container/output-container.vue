<template>
    <pre class="output-container" v-if="output" v-text="output">
    </pre>
</template>

<script>
export default {
    props: ['output'],
}
</script>

<style scoped>
.output-container{
    margin-bottom: 0;
    padding: .5em;
}
</style>