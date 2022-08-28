export class SocialPreviewHelper {
    constructor() {
        const head = document.getElementsByTagName("head")[0];
        const elements = this.getElements(head);
        this.elements = this.createMissingElements(elements, head);
        this.setInitialValues();
    }

    updateTitle(value) {
        this.elements.title.innerText = value;
    }

    updateDescription(value) {
        this.elements.description.innerText = value;
    }

    setInitialValues() {
        this.elements.url.innerText = `https://${document.location.host}/`;
        this.elements.image.innerText = `https://${document.location.host}/public/1024x1024.png`;
        this.updateDescription('Record Holcombe games, scores and player availability');
        this.updateTitle('Holcombe scores app');
    }

    createMissingElements(elements, head) {
        return {
            url: this.createMissingElement(elements.url, "og:url", head),
            title: this.createMissingElement(elements.title, "og:title", head),
            description: this.createMissingElement(elements.description, "og:description", head),
            image: this.createMissingElement(elements.image, "og:image", head),
        }
    }

    createMissingElement(element, property, head) {
        if (element !== null) {
            return element;
        }

        element = document.createElement("META");
        element.setAttribute("property", property);
        head.appendChild(element);

        return element;
    }

    getElements(head) {
        const metaTags = head.getElementsByTagName("meta");
        const elements = {
            url: null,
            title: null,
            description: null,
            image: null
        };

        for (let index = 0; index < metaTags.length; index++) {
            const metaTag = metaTags[index];
            const property = metaTag.getAttribute("property");
            switch (property) {
                case "og:url": {
                    elements.url = metaTag;
                    break;
                }
                case "og:title": {
                    elements.title = metaTag;
                    break;
                }
                case "og:description": {
                    elements.description = metaTag;
                    break;
                }
                case "og:image": {
                    elements.image = metaTag;
                    break;
                }
                default:
                    break;
            }
        }

        return elements;
    }
}