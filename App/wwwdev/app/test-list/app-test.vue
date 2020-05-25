<template>
    <router-link :to="myUri" class="app-test">
        <button class="btn btn-dark" :class="classFromStatus">
            <div>
                <fa-icon class="the-icon" icon="chevron-right" />
                <span class="name" v-text="test.name"></span>
            </div>
            <div class="right-box">
                <div class="status-background">
                    <span class="status-text" v-text="status"></span>
                </div>
                <button @click="showInfo" class="btn btn-sm btn-primary">
                    <fa-icon icon="info-circle" />
                </button>
            </div>
        </button>
    </router-link>
</template>

<script>
import alert from "../js/alert.js";
import api from "../js/api.js";

export default {
    props: ["test", "testFile"],
    computed: {
        status() {
            let status = this.test.runStatus;
            let result = this.test.testResult;
            switch (status) {
                case "WaitingOnParent":
                    if (!result.finished) {
                        return "Waiting on Compile";
                    } else {
                        if (result.success) {
                            return "Pass";
                        } else {
                            return "Fail";
                        }
                    }
            }
            return this.test.runStatus;
        },
        classFromStatus() {
            return {
                'passed': this.status === "Pass",
                'failed': this.status === "Fail",
                "btn-info": this.selected,
                "btn-dark": !this.selected
            };
        },
        myUri() {
            return `/${this.testFile.pathHash}/${this.test.testNumber}`;
        },
        selected() {
            return this.$route.params.testNumber == this.test.testNumber;
        }
    },
    methods: {
        showInfo(ev){
            api.get(`RootSourceFile/${this.testFile.pathHash}/${this.test.testNumber}`)
                .then(test => {
                    alert.alert(`${this.testFile.fileName} => ${this.test.name}`, JSON.stringify(test, null, '  '));
                });
            ev.stopPropagation();
            ev.preventDefault();
        }
    }
};
</script>

<style scoped>
.app-test {
    border-color: black;
    display: flex;
}

.app-test > button {
    padding: 0.25em;
    flex-grow: 1;

    display: flex;
    flex-flow: row nowrap;
    align-items: center;
    justify-content: space-between;
}

.right-box {
    display: flex;
    flex-flow: row nowrap;

    align-items: center;
}

.right-box > * {
    margin-left: .5em;
}

.status-background {
    background: #788086;
    border-radius: 0.5em;
    padding: 0 0.25em 0.1em 0.25em;
    text-shadow: black 1px 1px 2px;
    border: solid;
    border-width: 1px;
    border-color: darkgray;
}

.passed > div > div {
    border-color: #9bff9b;
}

.failed > div > div {
    border-color: #ffbfca;
}

.passed > div > div > .status-text {
    color: #9bff9b;
}

.failed > div > div > .status-text {
    color: #ffbfca;
}

.status-text {
    font-family: monospace;
    font-size: 1.25em;
    font-weight: bold;
}

.name {
    color: white;
}
</style>