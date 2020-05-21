<template>
    <div class="test-file">
        <router-link :to="testFileUrl">
            <button class="btn" :class="getBtnClass">
                <fa-icon v-if="selected" class="the-icon" icon="caret-square-down" />
                <fa-icon v-else class="the-icon" icon="caret-square-right" />
                <span class="file-name" v-text="testFile.fileName"></span>
                <span class="off-container">
                    <span class="off-text">
                        <span v-text="testFile.tests.length"></span> Tests
                    </span>
                    <div class="status-background" :class="getClassFromStatus">
                        <span v-text="status" class="status-text"></span>
                    </div>
                </span>
            </button>
        </router-link>
        <template v-if="selected">
            <app-test
                v-for="test in testFile.tests"
                :test="test"
                :testFile="testFile"
                :key="test.testNumber"
            ></app-test>
            <div class="footer"></div>
        </template>
    </div>
</template>

<script>
import AppTest from "./app-test.vue";

export default {
    props: ["testFile"],
    computed: {
        testFileUrl() {
            return `/${this.testFile.pathHash}`;
        },
        selected() {
            return this.$route.params.pathHash === this.testFile.pathHash;
        },
        getBtnClass() {
            return {
                "btn-secondary": !this.selected,
                "btn-info": this.selected
            };
        },
        status() {
            switch (this.testFile.compileStatus) {
                case "Failed":
                    return "Failed to Compile";
                case "Compiled":
                    let tests = this.testFile.tests;
                    if (tests.length === 0) {
                        return "No Tests";
                    }
                    if (tests.some(test => !test.testResult.finished)) {
                        return "Running";
                    }
                    let anySuccess = tests.some(
                        test => test.testResult.success
                    );
                    let anyFailure = tests.some(
                        test => !test.testResult.success
                    );
                    if (anySuccess && anyFailure) {
                        return "Partial Pass";
                    }
                    if (anySuccess) {
                        return "Pass";
                    }
                    if (anyFailure) {
                        return "Fail";
                    }
            }
            return this.testFile.compileStatus;
        },
        getClassFromStatus() {
            switch (this.status) {
                case "Failed to Compile":
                    return { "failed": true };
                case "Pass":
                    return { 'passed': true };
                case "Fail":
                    return { 'failed': true };
                case "Partial Pass":
                    return { "passed-partial": true };
            }
            return { "other": true };
        }
    },
    components: {
        AppTest
    }
};
</script>

<style scoped>
.test-file {
    display: flex;
    flex-flow: column nowrap;
}

.test-file > a {
    width: 100%;
    text-decoration: none;
}

.test-file > a > button {
    width: 100%;
    border-bottom: double;
    display: flex;
    flex-flow: row nowrap;
    align-items: center;
    justify-content: space-between;
}

.test-file > a > button > * {
    margin: 0 0.25em;
}

.file-name {
    font-size: 1.2em;
    margin-right: auto !important;
}

.off-container {
    display: flex;
    flex-flow: row nowrap;
    align-items: center;
}

.off-text {
    opacity: 0.7;
    font-size: 0.9em;
}

.the-icon {
    font-size: 1.5em;
}

.footer {
    width: 100%;
    border-bottom: double;
}

.status-background {
    margin-left: 1em;
    background-color: #272b30;
    border-radius: .5em;
    padding: 0 .25em .1em .25em;
    text-shadow: black 1px 1px 2px;
    border: solid;
    border-width: 1px;
    border-color: darkgray;
}

.failed {
    border-color: #ffd0d0;
    color: #ffd0d0;
}

.passed-partial {
    border-color: lightblue;
    color: lightblue;
}

.passed {
    border-color: #9bff9b;
    color: #9bff9b;
}

.status-text {
    font-size: 1em;
    font-weight: bold;
}
</style>