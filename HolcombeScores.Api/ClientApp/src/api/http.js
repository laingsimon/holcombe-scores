class Http {
    constructor(settings) {
        this.settings = settings;
        this.isTestingContextExpected = false;
    }

    expectTestingContext(expectation) {
        this.isTestingContextExpected = !!expectation;
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

    delete(relativeUrl, content) {
        return this.send('DELETE', relativeUrl, content ? content : null);
    }

    put(relativeUrl, content) {
        return this.send('PUT', relativeUrl, content);
    }

    async send(httpMethod, relativeUrl, content) {
        if (relativeUrl.indexOf('/') !== 0) {
            relativeUrl = '/' + relativeUrl;
        }

        const absoluteUrl = this.settings.apiHost + relativeUrl;

        if (content) {
            return fetch(absoluteUrl, {
                method: httpMethod,
                mode: 'cors',
                body: JSON.stringify(content),
                headers: {
                    'Content-Type': 'application/json',
                    'Testing-Context-Required': this.isTestingContextExpected
                },
                credentials: 'include'
            }).then(response => response.json());
        }

        return await fetch(absoluteUrl, {
            method: httpMethod,
            mode: 'cors',
            credentials: 'include',
            headers: {
                'Testing-Context-Required': this.isTestingContextExpected
            }
        })
            .then(response => response.json())
            .catch(e => console.error('ERROR: ' + e));
    }
}

export { Http };
