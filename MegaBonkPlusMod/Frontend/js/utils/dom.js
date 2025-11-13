export function getElem(id) {
    return document.getElementById(id);
}

export function getAll(selector) {
    return document.querySelectorAll(selector);
}

export function createElement(tag, attrs = {}, content = null) {
    const elem = document.createElement(tag);

    Object.entries(attrs).forEach(([key, value]) => {
        if (key === 'class') {
            elem.className = value;
        } else if (key === 'data') {
            Object.entries(value).forEach(([dataKey, dataValue]) => {
                elem.dataset[dataKey] = dataValue;
            });
        } else {
            elem.setAttribute(key, value);
        }
    });

    if (content) {
        if (typeof content === 'string') {
            elem.textContent = content;
        } else {
            elem.appendChild(content);
        }
    }

    return elem;
}

export function toggleClass(elem, className, force) {
    if (!elem) return;
    elem.classList.toggle(className, force);
}

export function on(elem, event, selectorOrHandler, handler) {
    if (typeof selectorOrHandler === 'function') {
        elem.addEventListener(event, selectorOrHandler);
    } else {
        elem.addEventListener(event, (e) => {
            const target = e.target.closest(selectorOrHandler);
            if (target) {
                handler.call(target, e);
            }
        });
    }
}