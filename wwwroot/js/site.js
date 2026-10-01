/**
 * site.js — Shared utilities
 */

// Format a number as EGP currency
function formatCurrency(amount) {
    return 'ج.م ' + parseFloat(amount).toFixed(2);
}

// Show a toast-style alert at top of screen
function showToast(message, type = 'success') {
    const existing = document.getElementById('site-toast');
    if (existing) existing.remove();

    const toast = document.createElement('div');
    toast.id = 'site-toast';
    toast.className = `alert alert-${type} fade-in`;
    toast.style.cssText = `
        position: fixed; top: 20px; right: 24px; z-index: 9999;
        min-width: 280px; max-width: 420px;
        box-shadow: 0 8px 24px rgba(0,0,0,0.15);
    `;
    toast.innerHTML = `
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" fill="none"
             viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          ${type === 'success'
              ? '<path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />'
              : '<path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />'}
        </svg>
        <span>${message}</span>
    `;
    document.body.appendChild(toast);
    setTimeout(() => { toast.style.opacity = '0'; toast.style.transition = 'opacity 0.4s'; }, 2800);
    setTimeout(() => toast.remove(), 3200);
}

// Activate tab system
function initTabs(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    const buttons = container.querySelectorAll('.tab-btn');
    const panels  = container.querySelectorAll('.tab-panel');

    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            buttons.forEach(b => b.classList.remove('active'));
            panels.forEach(p => p.classList.remove('active'));
            btn.classList.add('active');
            const target = btn.dataset.tab;
            const panel = container.querySelector(`.tab-panel[data-tab="${target}"]`);
            if (panel) panel.classList.add('active');
        });
    });

    // Activate first tab by default if none active
    if (!container.querySelector('.tab-btn.active') && buttons.length > 0)
        buttons[0].click();
}

// Confirm dialog wrapper
function confirmAction(message) {
    return confirm(message);
}

// Anti-forgery token helper for fetch POSTs
function getAntiForgeryToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

document.addEventListener('DOMContentLoaded', () => {
    // Auto-dismiss server-rendered alerts after 4 s
    document.querySelectorAll('.alert[data-auto-dismiss]').forEach(el => {
        setTimeout(() => {
            el.style.opacity = '0';
            el.style.transition = 'opacity 0.5s';
            setTimeout(() => el.remove(), 500);
        }, 4000);
    });
});
