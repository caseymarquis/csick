<template>
    <div v-if="testFile !== null" class="test-file">
        <div class="header-container">
            <h2 class="header">
                <span class="header-bright" v-text="testFile.fileName"></span>
            </h2>
        </div>
        <pre v-text="result.output"></pre>
    </div>
</template>

<script>
import Updates from "../../js/Updates.js";
import api from "../../js/api.js";

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
    components: {}
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

.code-container {
    overflow: scroll;
    flex-grow: 1;

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