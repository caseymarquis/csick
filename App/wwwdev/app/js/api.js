import axios from 'axios';

function getOrPost(url, payload, getPostEtc) {
    return getPostEtc(url, payload).then(function (response) {
        return response.data;
    });
}

const api = {
    get: function (url, payload) {
        return getOrPost(url, payload, axios.get);
    },
    post: function (url, payload) {
        return getOrPost(url, payload, axios.post);
    },
    put: function (url, payload) {
        return getOrPost(url, payload, axios.put);
    },
    delete: function (url, payload) {
        return getOrPost(url, payload, axios.delete);
    }
};

export default api;