<template>
 <div v-if="test !== null" class="test-results"> 
     <div class="header-container">
        <h2 class="header">
            <span class="header-bright" v-text="test.parent.fileName"></span>
            &nbsp;
            <fa-icon icon="arrow-right"/>
            &nbsp;
            <span class="header-bright" v-text="test.name"></span>
        </h2>
     </div>
     <div class="code-container">
         <div class="number-container">
            <div v-for="(line, $index) in test.parent.lines" :key="$index + 1" v-text="$index + 1">
            </div>
         </div>
         <div class="line-container">
            <code-line v-for="(line, $index) in test.parent.lines" :key="$index + 1" :line="line" :number="$index + 1" :test="test">
            </code-line>
         </div>
     </div>
 </div> 
</template>

<script>
import Updates from "../../js/Updates.js";
import api from "../../js/api.js";

import CodeLine from "./code-line.vue";

export default {
    data(){
        return {
            test: null,
        };
    },
    created(){
        this.fetchData();
    },
    watch: {
        '$route': 'fetchData',
    },
    computed: {
        result(){
            return (this.test && this.test.testResult) || {};
        }
    },
    methods: {
        fetchData(){
            api.get(`RootSourceFile/${this.$route.params.pathHash}/${this.$route.params.testNumber}`).then((test) => {
                this.test = test;
            });
        },
    },
    components: {
        CodeLine,
    }
}
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

.code-container {
    overflow: scroll;
    flex-grow: 1;

    display: flex;
    flex-flow: row nowrap;
}

.number-container{
    height: fit-content;
    background-color: #121417;
    border-right: solid;
    border-color: #696969;
    padding-right: .25em;
}

.line-container {
    height: fit-content;
    flex-grow: 1;
    padding-left: .25em;
    background-color: #121417;
}
</style>