<template>
 <div class="test-file"> 
     <router-link :to="testFileUrl">
        <button class="btn" :class="getBtnClass">
            <span class="file-name" v-text="testFile.fileName">
            </span>
            <span class="off-text">
                - <span v-text="testFile.tests.length"></span> Tests
                - <span v-text="testFile.compileStatus"></span>
            </span>
        </button>
     </router-link>
     <template v-if="selected">
         <app-test v-for="test in testFile.tests" :test="test" :key="test.number">
         </app-test>
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
}

.test-file > a > button {
    width: 100%;
    border-bottom: double;
}

.file-name{
    font-size: 1.2em;
}

.off-text{
    opacity: .7;;
    font-size: .9em;
}
</style>