class Http {
    static cache = {};
    static timeoutSecs = 60;
    
    constructor(settings) {
        this.settings = settings;
        this.cache = Http.cache;
        this.timeout = 1000 * Http.timeoutSecs;
    }

    static clearCache() {
        Http.cache = {};
    }

    get(relativeUrl) {
        return this.send('GET', relativeUrl, null);
    }

    getNoCache(relativeUrl) {
        return this.send('GET', relativeUrl, null, true);
    }

    post(relativeUrl, content) {
        return this.send('POST', relativeUrl, content);
    }

    patch(relativeUrl, content) {
        return this.send('PATCH', relativeUrl, content);
    }

    delete(relativeUrl) {
        return this.send('DELETE', relativeUrl, null);
    }

    put(relativeUrl, content) {
        return this.send('PUT', relativeUrl, content);
    }

    async send(httpMethod, relativeUrl, content, bypassCache) {
        if (relativeUrl.indexOf('/') !== 0) {
            relativeUrl = '/' + relativeUrl;
        }

        const absoluteUrl = this.settings.apiHost + relativeUrl;
        const match = (relativeUrl.match(/\/api\/([a-zA-Z]+)\/?/));
        let controller = match ? match[1] : 'unknown-' + relativeUrl;
        if (controller === 'My') {
            controller = 'Access';
        }

        if (httpMethod === 'GET') {
            const cache = this.cache[absoluteUrl];
            if (cache && cache.time + this.timeout > new Date().getTime()) {
                cache.reads++;
                return cache.data;
            }
        }
        else {
            Object.keys(this.cache).forEach(id => {
                if (this.cache[id].controller === controller || this.cache[id].controller === controller + 's' || this.cache[id].controller + 's' === controller) {
                    delete this.cache[id];
                }
            });
        }

        if (content) {
            return fetch(absoluteUrl, {
                method: httpMethod,
                mode: 'cors',
                body: JSON.stringify(content),
                headers: { 'Content-Type': 'application/json' },
                credentials: 'include'
            }).then(response => response.json());
        }

        const response = await fetch(absoluteUrl, {
            method: httpMethod,
            mode: 'cors',
            credentials: 'include'
        })
            .then(response => response.json())
            .catch(e => console.error("ERROR: " + e));

        if (httpMethod === 'GET') {
            this.cache[absoluteUrl] = {
                time: new Date().getTime(),
                data: response,
                reads: 0,
                controller: controller
            };
        }

        return response;
    }
}

export { Http };
