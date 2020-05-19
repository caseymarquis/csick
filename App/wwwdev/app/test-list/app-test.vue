<template>
<div class="app-test" :class="classFromStatus">
    <div>
        <fa-icon class="the-icon" icon="chevron-right"/>
        <span class="name" v-text="test.name">
        </span>
    </div>
    <span class="status-text" v-text="status">
    </span>
</div>
</template>

<script>
export default {
    props: ['test', 'testFile'],
    computed: {
        status(){
            let status = this.test.runStatus;
            let result = this.test.testResult;
            switch(status){
                case 'WaitingOnParent':
                    if(!result.finished){
                        return 'Waiting on Compile';
                    }
                    else{
                        if(result.success){
                            return 'Pass';
                        }
                        else{
                            return 'Fail';
                        }
                    }
            }
            return this.test.runStatus;
        },
        classFromStatus(){
            let status = this.status;
            if(status === 'Pass'){
                return {'passed': true }
            }
            else if(status === "Fail"){
                return {'failed': true }
            }
            return {};
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

.passed > .status-text {
    color: #9bff9b;
}

.failed > .status-text {
    color: #ffbfca;
}

.status-text {
    font-family: monospace;
    font-size: 1.25em;
}

.name{
    color: white;
}
</style>