class Http {
    constructor(settings) {
        this.settings = settings;
    }

    get(relativeUrl) {
        return this.send('GET', relativeUrl, null);
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

    send(httpMethod, relativeUrl, content) {
        if (relativeUrl.indexOf('/') !== 0) {
            relativeUrl = '/' + relativeUrl;
        }

        let absoluteUrl = this.settings.apiHost + relativeUrl;

        if (content) {
            return fetch(absoluteUrl, { method: httpMethod, body: JSON.stringify(content), headers: { 'Content-Type': 'application/json' } }).then(response => response.json());
        }

        return fetch(absoluteUrl).then(response => response.json());
    }
}