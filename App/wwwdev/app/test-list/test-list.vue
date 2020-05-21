<template>
    <div class="test-list">
        <div class="header-container">
            <h2 class="header">
                Available Test Files
            </h2>
        </div>
        <div class="test-list-container">
            <test-file v-for="testFile in testFiles" :testFile="testFile" :key="testFile.path"></test-file>
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
            testFiles: []
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
</style>