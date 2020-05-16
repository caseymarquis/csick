import 'babel-polyfill';

import './scss/main.scss';

import Vue from 'vue';
import VueRouter from 'vue-router';

//Import on main page load:
import MainRouter from './site-container/main-router.vue';

import { library } from '@fortawesome/fontawesome-svg-core'
import { 
    faSearch, faBars, faArrowLeft, faQuestion,
    faExclamation, faChevronDown, faChevronUp, faSort,
    faPlus, faTimes, faCalendarAlt
} from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon as FaIcon } from '@fortawesome/vue-fontawesome';
//Reference for component properties: https://github.com/FortAwesome/vue-fontawesome#basic
//Free solid icon list: https://fontawesome.com/icons?d=gallery&s=solid&m=free
Vue.component('fa-icon', FaIcon);
library.add(faSearch, faBars, faArrowLeft, faQuestion,
    faExclamation, faChevronDown, faChevronUp, faSort,
    faPlus, faTimes, faCalendarAlt);

//Import when navigating to page:
const MainLayout = () => import('./site-container/main-layout.vue');
const TestResults = () => import('./site-right/test-results/test-results.vue');

import Env from './js/Environment';
import store from './js/store.js';
import Updates from './js/Updates.js'; //Load signalR;

Vue.use(VueRouter);

Env.setApiRoot(null);

const router = new VueRouter({
    routes: [
        { path: "/", component : MainLayout,
            children : [
                { path: "", component : TestResults,
                },
            ]
        },
    ]
});

var vm = new Vue({
    el: '#router-mount',
    render: h => h(MainRouter),
    router,
    store
});