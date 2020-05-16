import axios from 'axios';
import moment from 'moment';

let Env = {
    apiRoot: "/api/v1",
    signalrRoot: "/updatehub",
    devServer: false,
    debug: false,
    setApiRoot(token) {
        axios.defaults.baseURL = `${Env.apiRoot}`
    },
    longAgo: new moment('10/20/1905'),
}

if (window.location.href.includes(":808")) {
    //Means we're using webpack dev server:
    //TODO: Check if port 8080 has a test API function on it, if so then the user set the port to 8080.
    let host = window.location.hostname;
    host = host.split(":")[0];
    Env.apiRoot = `http://${host}:5000/api/v1`;
    Env.signalrRoot = `http://${host}:5000/updatehub`;
    Env.devServer = true;
    Env.debug = true;
}

export default Env;