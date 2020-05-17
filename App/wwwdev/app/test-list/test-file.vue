<template>
 <div class="test-file"> 
     <router-link :to="testFileUrl">
        <button class="btn" :class="getBtnClass">
            <fa-icon v-if="selected" class="the-icon" icon="caret-square-down"/>
            <fa-icon v-else class="the-icon" icon="caret-square-right"/>
            <span class="file-name" v-text="testFile.fileName">
            </span>
            <span class="off-text">
                <span v-text="testFile.tests.length"></span> Tests
                - <span v-text="testFile.compileStatus"></span>
            </span>
        </button>
     </router-link>
     <template v-if="selected">
         <app-test v-for="test in testFile.tests" :test="test" :testFile="testFile" :key="test.number">
         </app-test>
         <div class="footer">
         </div>
     </template>
 </div>
</template>

<script>
import AppTest from "./app-test.vue";

export default {
    props: ['testFile'],
    computed: {
        testFileUrl(){
            return `/${this.testFile.path}`;
        },
        selected(){
            return this.$route.params.testFile === this.testFile.path;
        },
        getBtnClass(){
            return {
                'btn-secondary': !this.selected,
                'btn-info': this.selected,
            }
        },
    },
    components: {
        AppTest
    }
}
</script>

<style scoped>
.test-file{
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
    margin: 0 .25em;
}

.file-name{
    font-size: 1.2em;
}

.off-text{
    opacity: .7;;
    font-size: .9em;
}

.the-icon {
    font-size: 1.5em;
}

.footer{
    width: 100%;
    border-bottom: double;
}
</style>