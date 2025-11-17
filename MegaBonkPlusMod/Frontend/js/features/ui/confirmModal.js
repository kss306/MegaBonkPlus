import {createElement, on} from '../../utils/dom.js';

let backdrop = null;
let modal = null;
let titleElem = null;
let messageElem = null;
let buttonContainer = null;
let isOpen = false;

export function openConfirmModal({title, message, buttons}) {
    ensureModal();

    titleElem.textContent = title || 'Confirm';
    messageElem.innerHTML = message || '';

    buttonContainer.innerHTML = '';

    let resolvePromise;
    const promise = new Promise(resolve => {
        resolvePromise = resolve;
    });

    const close = (result) => {
        if (!isOpen) return;
        isOpen = false;
        backdrop.classList.add('is-hidden');
        resolvePromise?.(result);
    };

    (buttons || []).forEach((btn, index) => {
        const {
            label,
            variant = 'default',
            onClick,
            id = `btn_${index}`
        } = btn || {};

        const classNames = ['confirm-modal-button'];
        if (variant === 'primary') classNames.push('confirm-modal-button-primary');
        if (variant === 'danger') classNames.push('confirm-modal-button-danger');

        const buttonElem = createElement(
            'button',
            {type: 'button', class: classNames.join(' '), 'data-button-id': id},
            label || 'OK'
        );

        on(buttonElem, 'click', () => {
            try {
                onClick?.();
            } finally {
                close(id);
            }
        });

        buttonContainer.appendChild(buttonElem);
    });

    if (!buttons || buttons.length === 0) {
        const okBtn = createElement(
            'button',
            {type: 'button', class: 'confirm-modal-button confirm-modal-button-primary'},
            'OK'
        );
        on(okBtn, 'click', () => close('ok'));
        buttonContainer.appendChild(okBtn);
    }

    backdrop.classList.remove('is-hidden');
    isOpen = true;

    return promise;
}

function ensureModal() {
    if (backdrop) return;

    backdrop = createElement('div', {
        class: 'modal-backdrop',
        id: 'confirm-modal-backdrop'
    });

    modal = createElement('div', {
        class: 'modal-content card confirm-modal'
    });

    modal.innerHTML = `
        <div class="modal-header">
            <h3 id="confirm-modal-title">Confirm</h3>
            <button id="confirm-modal-close-btn" class="modal-close-btn">&times;</button>
        </div>
        <div class="confirm-modal-body">
            <div id="confirm-modal-message" class="confirm-modal-message"></div>
        </div>
        <div class="confirm-modal-footer" id="confirm-modal-footer"></div>
    `;

    titleElem = modal.querySelector('#confirm-modal-title');
    messageElem = modal.querySelector('#confirm-modal-message');
    buttonContainer = modal.querySelector('#confirm-modal-footer');

    const closeBtn = modal.querySelector('#confirm-modal-close-btn');

    const close = () => {
        if (!isOpen) return;
        isOpen = false;
        backdrop.classList.add('is-hidden');
    };

    on(backdrop, 'click', (e) => {
        if (e.target === backdrop) {
            close();
        }
    });

    on(closeBtn, 'click', close);

    backdrop.appendChild(modal);
    document.body.appendChild(backdrop);

    backdrop.classList.add('is-hidden');
}