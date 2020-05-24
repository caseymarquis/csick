<template>
    <div v-if="test !== null" class="test-results">
        <div class="header-container">
            <h2 class="header">
                <span class="header-bright" v-text="test.parent.fileName"></span>
                &nbsp;
                <fa-icon icon="arrow-right" />&nbsp;
                <span class="header-bright" v-text="test.name"></span>
            </h2>
        </div>
        <code-container class="the-code" :testFile="this.test.parent" :tests="[this.test]">
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
            test: null
        };
    },
    created() {
        this.fetchData();
        Updates.register(this, ['tests']);
    },
    destroyed(){
        Updates.remove(this);
    },
    watch: {
        $route: "fetchData"
    },
    computed: {
        result() {
            return (this.test && this.test.testResult) || {};
        }
    },
    methods: {
        fetchData() {
            api.get(
                `RootSourceFile/${this.$route.params.pathHash}/${this.$route.params.testNumber}`
            ).then(test => {
                this.test = test;
            });
        },
        processUpdate: function(group, cmd) {
            if (cmd === `${this.test.parent.path}/${this.test.testNumber}`) {
                this.fetchData();
            }
        }
    },
    components: {
        CodeContainer,
    }
};
</script>

<style scoped>
.test-results {
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