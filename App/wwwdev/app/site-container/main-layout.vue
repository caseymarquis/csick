<template>
    <div id="main-layout">
        <app-modal
            v-if="$store.state.modal.show"
            :title="$store.state.modal.title"
            :show-cancel="$store.state.modal.showCancel"
            width="60vw"
            v-on:close="closeMainModal"
        >
            <pre id="the-modal-pre" v-text="$store.state.modal.text"></pre>
        </app-modal>

        <slide-menu :style="slideDivStyle"></slide-menu>

        <div id="the-nav">
            <div class="nav-section">
                <button id="settings-toggle" class="btn btn-primary" v-on:click="toggleSlide">
                    <fa-icon icon="cogs" />
                </button>
                <router-link to="/" id="text-logo">
                    <h1>CSick</h1>
                    <!-- img :src="imgLogo" id="the-logo" / -->
                </router-link>
                <h2 id="page-title" class="visible-lg" v-text="pageTitle"></h2>
            </div>

            <search-bar
                id="nav-search"
                :wide="true"
                v-if="$store.state.search.show"
                v-model="searchText"
                placeholder="Search..."
            ></search-bar>

            <div class="nav-section"></div>
        </div>

        <div id="site-container" :style="mainDivStyle">
            <test-list id="test-list"></test-list>
            <router-view id="the-router"></router-view>
        </div>
    </div>
</template>

<script>
import AppModal from "../shared-components/app-modal.vue";
import SearchBar from "../shared-components/search-bar.vue";
import SlideMenu from "./slide-menu.vue";
import TestList from "../test-list/test-list.vue";
//import imgLogo from "../img/logo.png";

import SetupPage from "../js/SetupPage.js";
import router from "../js/router.js";

export default {
    data() {
        return {
            //imgLogo,
        };
    },
    created() {
        this.updateScreenHeight();
        setTimeout(() => {
            this.updateScreenHeight();
        }, 100);
        setInterval(() => {
            this.updateScreenHeight();
        }, 500);
    },
    computed: {
        pageTitle() {
            return this.$store.state.pageTitle;
        },
        searchText: {
            get() {
                return this.$store.state.search.text;
            },
            set(text) {
                if (text === undefined || text === null) {
                    return;
                }
                router.appendQuery(this, { search: text });
                this.$store.commit("setSearchText", text);
            }
        },
        mainDivStyle() {
            return { height: `${this.$store.state.screenHeight}px` };
        },
        slideDivStyle() {
            return { height: `${this.$store.state.screenHeight}px` };
        }
    },
    methods: {
        toggleSlide() {
            this.$store.commit("toggleSlide");
        },
        updateScreenHeight() {
            let navEl = document.getElementById("the-nav");
            if (!navEl) {
                return;
            }
            let newVal = window.innerHeight - navEl.clientHeight;
            if (newVal !== this.$store.state.screenHeight) {
                this.$store.commit("setScreenHeight", newVal);
            }
        },
        closeMainModal(){
            this.$store.commit('setModal', { show: false });
        }
    },
    components: {
        SearchBar,
        SlideMenu,
        TestList,
        AppModal,
    }
};
</script>

<style scoped>
#main-layout {
    display: flex;
    flex-flow: column nowrap;
    padding-top: 0;
}

#print-header {
    font-size: xx-large;
    margin-left: 10px;
    display: inline-block;
    border-bottom: solid;
}

#the-nav {
    display: none !important;
    position: fixed;
    top: 0;
    z-index: 5;
    width: 100%;
    height: 50px;
    display: flex;
    flex-flow: row nowrap;
    justify-content: space-between;
    align-items: center;
    background-color: #2c3e50;
}

.nav-section {
    display: flex;
    flex-flow: row nowrap;
    align-items: center;
    justify-content: space-around;
}

#text-logo,
#text-logo > h1 {
    color: white;
    text-decoration: none;
    font-size: 1.25em;
}

#text-logo:hover {
    text-decoration: none;
}

#nav-search {
    flex-shrink: 1;
    max-width: 400px;
}

.nav-section > * {
    margin-right: 1em;
}

.nav-link {
    text-decoration: none;
    color: white;
    font-weight: normal;
}

.nav-link:hover {
    color: lightgray;
}

#report-title {
    margin-left: 2em;
    margin-bottom: 0;
    font-weight: bold;
    font-size: x-large;
}

#page-title {
    color: white;
    text-shadow: black 2px 2px;
    font-size: x-large;
    border-top: solid;
    border-bottom: solid;
    border-width: 1px;
}

#site-container {
    position: relative;
    display: flex;
    flex-flow: row nowrap;
}

#test-list {
    height: 100%;
}

#the-router {
    height: 100%;
    flex-grow: 1;
}

#the-modal-pre {
    max-width: 60vw;
    max-height: 60vh;
    overflow: scroll;
    background: #111;
}

@media (max-width: 720px) {
    #the-logo {
        display: none;
    }
}
@media (max-width: 1200px) {
    #the-logo {
        height: 35px;
    }
    .nav-link {
        font-size: 1em;
    }
}
@media (min-width: 1200px) {
    #the-logo {
        height: 45px;
    }
    .nav-link {
        font-size: 1.25em;
    }
}
</style>