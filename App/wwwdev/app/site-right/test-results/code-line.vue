<template>
    <div class="code-line" :class="getClass">
        <span v-text="line"></span>
    </div>
</template>

<script>
export default {
    props: ["line", "number", "test"],
    computed: {
        getClass() {
            let isFailedAssert =
                this.number === this.exitCode &&
                this.exitCode !== 0;

            if (isFailedAssert) {
                return { "highlight-bad": true };
            } else if (this.number === this.test.lineNumber) {
                if (this.test.testResult.finished) {
                    if (this.test.testResult.success) {
                        return { "highlight-good": true };
                    } else {
                        return { "highlight-bad": true };
                    }
                }
                else{
                    return { "highlight-test": true };
                }
            }
        },
        exitCode() {
            return this.test.testResult.exitCode;
        }
    }
};
</script>

<style scoped>
.code-line {
    white-space: pre;
    padding-top: 0;
}

.highlight-bad {
    background: #222;
    color: lightcoral;
}

.highlight-good {
    background: #222;
    color: lightgreen;
}

.highlight-test {
    background: #222;
    color: lightskyblue;
}
</style>