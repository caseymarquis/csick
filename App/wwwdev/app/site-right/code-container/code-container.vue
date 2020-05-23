<template>
    <div class="code-container">
        <div class="number-container">
            <div
                v-for="(line, $index) in testFile.lines"
                :key="$index + 1"
                v-text="$index + 1"
            ></div>
        </div>
        <div class="line-container">
            <code-line
                v-for="(line, $index) in testFile.lines"
                :key="$index + 1"
                :line="line"
                :number="$index + 1"
                :tests="tests"
            ></code-line>
        </div>
    </div>
</template>

<script>
import CodeLine from "./code-line.vue";
export default {
    props: ["testFile", "tests"],
    components: {
        CodeLine,
    }
};
</script>

<style scoped>
.code-container {
    display: flex;
    flex-flow: row nowrap;
}

.number-container {
    height: fit-content;
    background-color: #121417;
    border-right: solid;
    border-color: #696969;
    padding-right: 0.25em;
}

.line-container {
    height: fit-content;
    flex-grow: 1;
    padding-left: 0.25em;
    background-color: #121417;
}
</style>