import axios from 'axios'

const instance = axios.create ({
    baseURL: 'http://localhost:56886/',
    timeout: 50000
})

const getResource = (url) => {
    return instance.get(url)
        .then(result => {
            return result.data
        })
        .catch(error => {
            // eslint-disable-next-line no-console
            console.log(error)
            return error
        })
}

export default {
    getResource
}