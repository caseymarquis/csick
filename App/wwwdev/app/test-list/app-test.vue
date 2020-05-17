<template>
<div class="app-test">
    <div>
        <fa-icon class="the-icon" icon="chevron-right"/>
        <span class="name" v-text="test.name">
        </span>
    </div>
    <span v-text="runStatus">
    </span>
</div>
</template>

<script>
export default {
    props: ['test', 'testFile'],
    computed: {
        runStatus(){
            if(this.testFile.compileStatus !== 'Compiled'){
                return 'Waiting on Compile'
            }
            switch(this.test.runStatus){
                case 'NotRun': return 'Not Run';
            }
            return this.test.runStatus;
        }
    }
}
</script>

<style scoped>
.app-test {
    border-bottom: solid;
    padding: .25em;

    display: flex;
    flex-flow: row nowrap;
    align-items: center;
    justify-content: space-between;
}

.name{
    color: white;
}
</style>