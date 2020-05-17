<template>
 <div class="test-list"> 
     <h2 class="header">
         Available Test Files
     </h2>
     <div class="test-list-container">
        <test-file v-for="testFile in testFiles" :testFile="testFile" :key="testFile.path">
        </test-file>
     </div>
 </div> 
</template>

<script>
import api from "../js/api.js";
import TestFile from "./test-file.vue";

export default {
    data(){
        return {
            testFiles: [],
        }
    },
    created(){
        this.fetchData();
    },
    methods: {
        fetchData(){
            api.get(`RootSourceFile`).then(testFiles => {
                this.testFiles = testFiles;
            });
        }
    },
    components: {
        TestFile,
    }
}
</script>

<style scoped>
.test-list{
    display: flex;
    flex-flow: column nowrap;
    height: 100%;

    border-right: solid;
    border-width: 2px;
    min-width: 20%;
}

.test-list-container {
    flex-grow: 1;

    display: flex;
    flex-flow: column nowrap;
    overflow-y: scroll;
}

.header{
    padding: .25em .5em;
    margin: 0;
    border-bottom: solid;
    border-width: 3px;
    color: lightgray;

    display: flex;
    justify-content: center;
}
</style>