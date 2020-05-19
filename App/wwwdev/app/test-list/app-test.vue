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
            let status = this.test.runStatus;
            let result = this.test.testResult;
            switch(status){
                case 'WaitingOnParent':
                    if(!result.finished){
                        return 'Waiting on Compile';
                    }
                    else{
                        if(result.success){
                            return 'Passed';
                        }
                        else{
                            return 'Failed';
                        }
                    }
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