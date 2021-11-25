<template>
    <div class="test-list">
        <div class="header-container">
            <h2 class="header">Available Test Files</h2>
        </div>
        <div v-if="testFiles.length > 0" class="test-list-container">
            <test-file v-for="testFile in testFiles" :testFile="testFile" :key="testFile.path"></test-file>
        </div>
        <div v-else class="test-list-container">
            <p class="error-message" v-for="path in testDirectories" :key="path.noKey">
                No test files found in
                '<span v-text="path">
                </span>'.
                You'll need to either add tests in this location, or change the
                default test location. Relative to the csick directory, look for
                '../csick-data/config.json'. You can change the test directories
                from here. Refer to 'csick/example-project/.vscode' for debugging
                tests from vscode. Example tests can be found in 'csick/example-project/test'.
            </p>
        </div>
    </div>
</template>

<script>
import api from "../js/api.js";
import TestFile from "./test-file.vue";
import Updates from "../js/Updates.js";

export default {
    data() {
        return {
            testFiles: [],
            testDirectories: [],
        };
    },
    created() {
        Updates.register(this, ["tests"]);
        this.fetchData();
    },
    destroyed() {
        Updates.remove(this);
    },
    methods: {
        fetchData() {
            api.get(`RootSourceFile`).then(testFiles => {
                this.testFiles = testFiles;
                if(testFiles.length === 0){
                    api.get(`Diag/Directories`).then(testDirectories => {
                        this.testDirectories = testDirectories;
                    });
                }
            });
        },
        processUpdate: function(group, cmd) {
            if (group === "tests") {
                this.fetchData();
            }
        }
    },
    components: {
        TestFile
    }
};
</script>

<style scoped>
.test-list {
    display: flex;
    flex-flow: column nowrap;
    height: 100%;
    background-color: #212121;

    border-width: 2px;
    min-width: 20%;
}

.test-list-container {
    flex-grow: 1;

    display: flex;
    flex-flow: column nowrap;
    overflow-y: scroll;
}

.header-container {
    background-color: #212121;
    width: 100%;
    border-bottom: double;
    border-right: solid;
    border-right-width: 1px;
}

.header {
    display: flex;
    justify-content: center;
}

.header-bright {
    color: white;
}

.error-message {
    padding: .25em;
    border-bottom: solid;
    max-width: 40em;
}
</style>