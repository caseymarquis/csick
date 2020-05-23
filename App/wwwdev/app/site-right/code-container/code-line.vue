<template>
    <div class="code-line" :class="getClass">
        <span v-text="line"></span>
    </div>
</template>

<script>
export default {
    props: ["line", "number", "tests"],
    computed: {
        getClass() {
            for(let i = 0; i < this.tests.length; i++){
                let test = this.tests[i];
                let exitCode = test.testResult.exitCode;
                let isFailedAssert =
                    this.number === exitCode &&
                    exitCode !== 0;

                if (isFailedAssert) {
                    return { "highlight-bad": true };
                } else if (this.number === test.lineNumber) {
                    if (test.testResult.finished) {
                        if (test.testResult.success) {
                            return { "highlight-good": true };
                        } else {
                            return { "highlight-bad": true };
                        }
                    }
                    else{
                        return { "highlight-test": true };
                    }
                }
            }
        },
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