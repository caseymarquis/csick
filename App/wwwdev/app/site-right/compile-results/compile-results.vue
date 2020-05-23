<template>
    <div v-if="testFile !== null" class="test-file">
        <div class="header-container">
            <h2 class="header">
                <span class="header-bright" v-text="testFile.fileName"></span>
            </h2>
        </div>
        <pre v-text="result.output"></pre>
        <code-container class="the-code" :testFile="this.testFile" :tests="this.testFile.tests">
        </code-container>
    </div>
</template>

<script>
import Updates from "../../js/Updates.js";
import api from "../../js/api.js";

import CodeContainer from "../code-container/code-container.vue";

export default {
    data() {
        return {
            testFile: null
        };
    },
    created() {
        this.fetchData();
    },
    watch: {
        $route: "fetchData"
    },
    computed: {
        result() {
            return (this.testFile && this.testFile.compileResult) || {};
        }
    },
    methods: {
        fetchData() {
            api.get(`RootSourceFile/${this.$route.params.pathHash}`).then(
                testFile => {
                    this.testFile = testFile;
                }
            );
        }
    },
    components: {
        CodeContainer,
    }
};
</script>

<style scoped>
.test-file {
    display: flex;
    flex-flow: column nowrap;
}

.header-container {
    background-color: #212121;
    width: 100%;
    border-bottom: double;
    padding-left: 1em;
}

.header {
    display: flex;
    flex-flow: row wrap;
    align-items: center;
}

.header-bright {
    color: white;
}

.the-code {
    overflow: scroll;
    flex-grow: 1;
}
</style>