const MAX_VISIBLE_TOASTS = 3;

let toastQueue = [];
let visibleToasts = 0;
let containerEl = null;

function ensureContainer() {
    if (containerEl) return containerEl;

    containerEl = document.getElementById('toast-container');
    if (!containerEl) {
        containerEl = document.createElement('div');
        containerEl.id = 'toast-container';
        document.body.appendChild(containerEl);
    }
    return containerEl;
}

export function showToast(type, message, duration = 3000) {
    toastQueue.push({type, message, duration});
    processQueue();
}

function processQueue() {
    if (visibleToasts >= MAX_VISIBLE_TOASTS) return;
    if (toastQueue.length === 0) return;

    const {type, message, duration} = toastQueue.shift();
    showOneToast(type, message, duration);
}

function showOneToast(type, message, duration) {
    visibleToasts++;
    const container = ensureContainer();

    const toast = document.createElement('div');
    toast.className = `toast toast--${type}`;

    const content = document.createElement('div');
    content.className = 'toast__content';
    content.textContent = message;

    const closeBtn = document.createElement('button');
    closeBtn.className = 'toast__close';
    closeBtn.innerHTML = '&times;';

    const progress = document.createElement('div');
    progress.className = 'toast__progress';

    toast.appendChild(content);
    toast.appendChild(closeBtn);
    toast.appendChild(progress);
    container.appendChild(toast);

    requestAnimationFrame(() => {
        toast.classList.add('toast--show');
        progress.style.transition = `width ${duration}ms linear`;
        void progress.offsetWidth;
        progress.style.width = '0%';
    });

    let hideTimeout = null;
    let removeTimeout = null;

    const cleanup = () => {
        if (!toast.parentElement) return;
        toast.classList.remove('toast--show');
        toast.classList.add('toast--hide');
        clearTimeout(hideTimeout);
        clearTimeout(removeTimeout);

        removeTimeout = setTimeout(() => {
            if (toast.parentElement) toast.parentElement.removeChild(toast);
            visibleToasts = Math.max(0, visibleToasts - 1);
            processQueue();
        }, 250);
    };

    hideTimeout = setTimeout(cleanup, duration);

    closeBtn.addEventListener('click', () => {
        cleanup();
    });
}