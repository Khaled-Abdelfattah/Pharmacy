/**
 * barcode.js — Barcode scanner input handler
 *
 * A barcode scanner acts as a fast keyboard that types a string
 * and sends Enter/CR. This module distinguishes scanner input
 * (fast burst of characters) from manual typing (slow keystrokes).
 *
 * Usage:
 *   BarcodeScanner.init({
 *     onScanned: (barcode) => { ... }
 *   });
 */

const BarcodeScanner = (() => {
    const SCAN_THRESHOLD_MS = 50;   // chars faster than this = scanner
    const MIN_BARCODE_LEN   = 3;    // ignore very short bursts

    let buffer = '';
    let lastKeyTime = 0;
    let isScanning = false;
    let _onScanned = null;

    function handleKeyDown(e) {
        const now = Date.now();
        const elapsed = now - lastKeyTime;
        lastKeyTime = now;

        // If typing fast (scanner pace), accumulate characters
        if (elapsed < SCAN_THRESHOLD_MS) {
            isScanning = true;
        } else {
            // Slow keystroke — could be the start of manual entry or a new scan
            if (isScanning && buffer.length >= MIN_BARCODE_LEN) {
                // We had a scan in progress — flush it
                submitBarcode(buffer);
                buffer = '';
                isScanning = false;
                return;
            }
            buffer = '';
            isScanning = false;
        }

        if (e.key === 'Enter' || e.key === 'Tab') {
            if (buffer.length >= MIN_BARCODE_LEN) {
                e.preventDefault();
                submitBarcode(buffer);
                buffer = '';
                isScanning = false;
            }
            return;
        }

        // Accumulate printable characters only
        if (e.key.length === 1) {
            buffer += e.key;
        }
    }

    function submitBarcode(code) {
        const clean = code.trim();
        if (clean.length >= MIN_BARCODE_LEN && typeof _onScanned === 'function') {
            _onScanned(clean);
        }
    }

    return {
        init(options) {
            _onScanned = options.onScanned;
            document.addEventListener('keydown', handleKeyDown);
        },
        destroy() {
            document.removeEventListener('keydown', handleKeyDown);
        }
    };
})();
