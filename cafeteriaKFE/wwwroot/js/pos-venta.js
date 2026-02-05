(() => {
    console.log('pos-venta.js loaded and executing');
    // --------------------------
    // Helpers
    // --------------------------
    function money(n) {
        return new Intl.NumberFormat('es-MX', { style: 'currency', currency: 'MXN' }).format(n);
    }

    async function httpGetJson(url) {
        const res = await fetch(url, { headers: { "Accept": "application/json" } });
        if (!res.ok) throw new Error(`HTTP ${res.status}`);
        return await res.json();
    }

    async function httpPostJson(url, body) {
        const res = await fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/json", "Accept": "application/json" },
            body: JSON.stringify(body)
        });
        if (!res.ok) {
            const txt = await res.text();
            throw new Error(txt || `HTTP ${res.status}`);
        }
        return await res.json();
    }

    // --------------------------
    // DOM
    // --------------------------
    const productSearch = document.getElementById('productSearch');
    const btnSearch = document.getElementById('btnSearch');
    const searchError = document.getElementById('searchError');
    const searchResultsWrap = document.getElementById('searchResultsWrap');
    const searchResults = document.getElementById('searchResults');
    const btnClearResults = document.getElementById('btnClearResults');
    

    const cartBody = document.getElementById('cartBody');
    const itemsCount = document.getElementById('itemsCount');
    const subtotalEl = document.getElementById('subtotal');
    const discountEl = document.getElementById('discount');
    const totalEl = document.getElementById('total');
    const btnClearCart = document.getElementById('btnClearCart');
    const btnPay = document.getElementById('btnPay');
    const isDelivery = document.getElementById('isDelivery');

    // Config modal
    const configModalEl = document.getElementById('configModal');
    const configModal = new bootstrap.Modal(configModalEl);
    const cfgProductName = document.getElementById('cfgProductName');
    const cfgProductCode = document.getElementById('cfgProductCode');
    const cfgError = document.getElementById('cfgError');
    const cfgSize = document.getElementById('cfgSize');
    const cfgMilk = document.getElementById('cfgMilk');
    const cfgQty = document.getElementById('cfgQty');
    const cfgUnitPrice = document.getElementById('cfgUnitPrice');
    const cfgLineTotal = document.getElementById('cfgLineTotal');
    const btnCfgAdd = document.getElementById('btnCfgAdd');
    const cfgTemp = document.getElementById('cfgTemp');
    const cfgSyrup = document.getElementById('cfgSyrup');
    const cfgWhip = document.getElementById('cfgWhip');
    const tempSection = document.getElementById('tempSection');
    const syrupSection = document.getElementById('syrupSection');
    const whipSection = document.getElementById('whipSection');

    // Pay modal
    const payModalEl = document.getElementById('payModal');
    const payCash = document.getElementById('payCash');
    const payCard = document.getElementById('payCard');
    const cashSection = document.getElementById('cashSection');
    const cardSection = document.getElementById('cardSection');
    const cashReceived = document.getElementById('cashReceived');
    const cashChange = document.getElementById('cashChange');
    const btnCashConfirm = document.getElementById('btnCashConfirm');
    const cardProcessing = document.getElementById('cardProcessing');
    const cardSuccess = document.getElementById('cardSuccess');
    const payError = document.getElementById('payError');
    const paySuccess = document.getElementById('paySuccess');

    // --------------------------
    // State
    // --------------------------
    let cart = [];
    let optionsCache = null; // { sizes:[], milkTypes:[] }
    let selectedProduct = null; // { productId, barCode, name, basePrice }

    const TYPE_TEMP = 3;
    const TYPE_SYRUP = 2;
    const TYPE_WHIP = 4;
    const FOOD_TYPE_ID = 5; // si ya existe, deja el tuyo
    const WHIPPED_CREAM_PRICE = 0; // si no tiene costo, 0


    // --------------------------
    // Cart rendering
    // --------------------------
    function calcTotals() {
        const sub = cart.reduce((acc, i) => acc + i.unitPrice * i.qty, 0);
        const discount = 0;
        const total = sub - discount;

        subtotalEl.textContent = money(sub);
        discountEl.textContent = money(discount);
        totalEl.textContent = money(total);

        itemsCount.textContent = `${cart.reduce((a, i) => a + i.qty, 0)} items`;
        return { sub, discount, total };
    }

    function applyTypeUI(typeId) {
        tempSection?.classList.toggle('d-none', typeId !== TYPE_TEMP);
        syrupSection?.classList.toggle('d-none', typeId !== TYPE_SYRUP);
        whipSection?.classList.toggle('d-none', typeId !== TYPE_WHIP);

        // reset para no arrastrar valores
        if (typeId !== TYPE_TEMP && cfgTemp) cfgTemp.selectedIndex = 0;
        if (typeId !== TYPE_SYRUP && cfgSyrup) cfgSyrup.selectedIndex = 0;
        if (typeId !== TYPE_WHIP && cfgWhip) cfgWhip.checked = false;
    }


    function renderCart() {
        if (cart.length === 0) {
            cartBody.innerHTML = `
                <tr class="text-muted">
                    <td colspan="5" class="text-center py-4">Aún no hay productos en el carrito.</td>
                </tr>`;
            calcTotals();
            return;
        }

        cartBody.innerHTML = cart.map((i, idx) => `
            <tr>
                <td>
                    <div class="fw-semibold">${i.name}</div>
                    <div class="text-muted small">
                        BarCode: ${i.barCode} • Size: ${i.sizeName} • Milk: ${i.milkName}
                        • Size: ${i.sizeName}
                        • Milk: ${i.milkName}
                        ${i.temperatureId ? `• Temp: ${i.tempName}` : ''}
                        ${i.syrupId ? `• Syrup: ${i.syrupName}` : ''}
                        ${(i.hasWhippedCream !== undefined && i.hasWhippedCream !== null) ? `• Crema: ${i.hasWhippedCream ? 'Sí' : 'No'}` : ''}
                    </div>
                </td>
                <td class="text-end">${money(i.unitPrice)}</td>
                <td class="text-center">
                    <div class="btn-group btn-group-sm" role="group">
                        <button class="btn btn-outline-secondary" data-action="dec" data-idx="${idx}">-</button>
                        <button class="btn btn-outline-secondary" disabled>${i.qty}</button>
                        <button class="btn btn-outline-secondary" data-action="inc" data-idx="${idx}">+</button>
                    </div>
                </td>
                <td class="text-end fw-semibold">${money(i.unitPrice * i.qty)}</td>
                <td class="text-end">
                    <button class="btn btn-outline-danger btn-sm" data-action="remove" data-idx="${idx}">✕</button>
                </td>
            </tr>
        `).join('');

        calcTotals();
    }

    function addToCart(line) {
        // Merge si mismo producto + mismas opciones
        const existing = cart.find(x =>
            x.productId === line.productId &&
            x.sizeId === line.sizeId &&
            x.milkTypeId === line.milkTypeId
        );

        if (existing) existing.qty += line.qty;
        else cart.push(line);

        renderCart();
    }

    // --------------------------
    // Search
    // --------------------------
    function setSearchError(msg) {
        if (!msg) {
            searchError.classList.add('d-none');
            searchError.textContent = '';
            return;
        }
        searchError.classList.remove('d-none');
        searchError.textContent = msg;
    }

    function showResults(list) {
        if (!list || list.length === 0) {
            searchResultsWrap.classList.add('d-none');
            searchResults.innerHTML = '';
            return;
        }

        searchResultsWrap.classList.remove('d-none');
        searchResults.innerHTML = list.map(p => `
            <button type="button" class="list-group-item list-group-item-action"
                    data-product-id="${p.productId}">
                <div class="d-flex justify-content-between">
                    <div class="fw-semibold">${p.name}</div>
                    <small class="text-muted">${money(p.basePrice)}</small>
                </div>
                <small class="text-muted">BarCode: ${p.barCode}</small>
            </button>
        `).join('');
    }

    async function searchAndHandle(query) {
        const q = (query || '').trim();
        setSearchError(null);
        showResults([]);

        if (!q) return;

        try {
            const results = await httpGetJson(`/Pos/Search?query=${encodeURIComponent(q)}`);

            if (!results || results.length === 0) {
                setSearchError("No se encontraron productos.");
                return;
            }

            // Si solo hay 1, abrimos configuración directo
            if (results.length === 1) {
                handleProductSelected(results[0]);
                return;
            }


            // Si hay varios: mostrar lista
            showResults(results);
        } catch (e) {
            setSearchError("Error al buscar. Revisa que /Pos/Search esté funcionando.");
        }
    }

    btnSearch.addEventListener('click', () => searchAndHandle(productSearch.value));
    productSearch.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            searchAndHandle(productSearch.value);
        }
    });

    btnClearResults.addEventListener('click', () => {
        productSearch.value = '';
        showResults([]);
        setSearchError(null);
    });

    searchResults.addEventListener('click', (e) => {
        const btn = e.target.closest('button[data-product-id]');
        if (!btn) return;

        const productId = Number(btn.getAttribute('data-product-id'));
        const p = lastSearchResults.find(x => x.productId === productId);
        if (!p) return;

        handleProductSelected(p);
    });


    // Guardamos la última lista de resultados para seleccionar bien:
    let lastSearchResults = [];
    const originalShowResults = showResults;
    showResults = function(list) {
        lastSearchResults = list || [];
        originalShowResults(list);
    };

    // --------------------------
    // Options & Config Modal
    // --------------------------
    async function ensureOptionsLoaded() {
        if (optionsCache) return optionsCache;
        optionsCache = await httpGetJson('/Pos/Options'); // { sizes, milkTypes }
        return optionsCache;
    }

    function fillSelect(selectEl, items) {
        selectEl.innerHTML = items.map(x =>
            `<option value="${x.id}" data-delta="${x.priceDelta}">${x.name} (${money(x.priceDelta)})</option>`
        ).join('');
    }

    function getSelectedDelta(selectEl) {
        const opt = selectEl.options[selectEl.selectedIndex];
        if (!opt) return 0;
        return Number(opt.getAttribute('data-delta') || 0);
    }

    function updateConfigPrices() {
        if (!selectedProduct) return;

        const typeId = Number(selectedProduct.productTypeId);
        const sizeDelta = getSelectedDelta(cfgSize);
        const milkDelta = getSelectedDelta(cfgMilk);
        const qty = Math.max(1, Number(cfgQty.value || 1));

        let unit = Number(selectedProduct.basePrice) + sizeDelta + milkDelta;

        if (typeId === TYPE_SYRUP) {
            unit += getSelectedDelta(cfgSyrup);
        }

        // Type 4: Whipped Cream (costo fijo si aplica)
        if (typeId === TYPE_WHIP && cfgWhip?.checked) {
            unit += WHIPPED_CREAM_PRICE;
        }

        cfgUnitPrice.textContent = money(unit);
        cfgLineTotal.textContent = money(unit * qty);
    }

    function isFood(p) {
        return Number(p.productTypeId) === FOOD_TYPE_ID;
    }

    async function handleProductSelected(p) {
        if (!p) return;
        console.log('handleProductSelected - Product:', p);

        const typeId = Number(p.productTypeId);
        console.log('handleProductSelected - productTypeId:', typeId);

        // Seguridad: si no viene el tipo, trata como "requiere configuración"
        if (Number.isNaN(typeId)) {
            await openConfig(p);
            return;
        }

        // FOOD: se agrega directo, sin modal
        if (typeId === FOOD_TYPE_ID) {
            addToCart({
                productId: p.productId,
                barCode: p.barCode,
                productTypeId: typeId,
                name: p.name,
                basePrice: Number(p.basePrice),
                unitPrice: Number(p.basePrice),
                qty: 1,

                // No aplica en Food:
                sizeId: null,
                milkTypeId: null,
                temperatureId: null,
                syrupId: null,
                hasWhippedCream: false,

                sizeName: 'N/A',
                milkName: 'N/A',
                tempName: 'N/A',
                syrupName: 'N/A',
                whipName: 'N/A'
            });

            // Limpieza UX
            showResults([]);
            setSearchError(null);
            productSearch.value = '';
            return;
        }

        // NO FOOD: requiere configuración (Size + Milk + extras según tipo)
        await openConfig(p);

        // Limpieza UX (opcional, pero deja la pantalla lista para el siguiente escaneo)
        showResults([]);
        setSearchError(null);
        productSearch.value = '';
    }


    async function openConfig(product) {
        selectedProduct = product;
        cfgError.classList.add('d-none');
        cfgError.textContent = '';

        cfgProductName.textContent = product.name;
        cfgProductCode.textContent = `BarCode: ${product.barCode}`;

        try {
            const opts = await ensureOptionsLoaded();
            fillSelect(cfgSize, opts.sizes || []);
            fillSelect(cfgMilk, opts.milkTypes || []);
            fillSelect(cfgTemp, opts.temperatures || []);
            fillSelect(cfgSyrup, opts.syrups || []);

            cfgWhip && (cfgWhip.checked = false);
            applyTypeUI(Number(product.productTypeId));

            cfgQty.value = 1;

            updateConfigPrices();
            console.log('openConfig - About to show configModal');
            configModal.show();

            // Limpiamos resultados para no estorbar
            showResults([]);
            setSearchError(null);
            productSearch.value = '';
        } catch (e) {
            console.error('openConfig - Error during modal configuration:', e);
            cfgError.classList.remove('d-none');
            cfgError.textContent = "No se pudieron cargar/mostrar opciones del modal. Revisa IDs del HTML y /Pos/Options.";
        }
    }

    cfgSize.addEventListener('change', updateConfigPrices);
    cfgMilk.addEventListener('change', updateConfigPrices);
    cfgQty.addEventListener('input', updateConfigPrices);
    cfgTemp && cfgTemp.addEventListener('change', updateConfigPrices);
    cfgSyrup && cfgSyrup.addEventListener('change', updateConfigPrices);
    cfgWhip && cfgWhip.addEventListener('change', updateConfigPrices);


    btnCfgAdd.addEventListener('click', () => {
        if (!selectedProduct) return;

        cfgError.classList.add('d-none');
        cfgError.textContent = '';

        const typeId = Number(selectedProduct.productTypeId);

        const qty = Math.max(1, Number(cfgQty.value || 1));
        const sizeId = Number(cfgSize.value);
        const milkTypeId = Number(cfgMilk.value);

        // Para TODO lo que NO es food, pedimos Size + Milk (como definiste)
        if (!sizeId || !milkTypeId) {
            cfgError.classList.remove('d-none');
            cfgError.textContent = "Selecciona Size y MilkType.";
            return;
        }

        // Extras según tipo
        const temperatureId = (typeId === TYPE_TEMP) ? Number(cfgTemp?.value) : null;
        const syrupId = (typeId === TYPE_SYRUP) ? Number(cfgSyrup?.value) : null;
        const hasWhippedCream = (typeId === TYPE_WHIP) ? !!cfgWhip?.checked : false;

        // Validaciones obligatorias
        if (typeId === TYPE_TEMP && (!temperatureId || Number.isNaN(temperatureId))) {
            cfgError.classList.remove('d-none');
            cfgError.textContent = "Selecciona una Temperatura.";
            return;
        }

        if (typeId === TYPE_SYRUP && (!syrupId || Number.isNaN(syrupId))) {
            cfgError.classList.remove('d-none');
            cfgError.textContent = "Selecciona un Jarabe.";
            return;
        }

        const sizeOpt = cfgSize.options[cfgSize.selectedIndex];
        const milkOpt = cfgMilk.options[cfgMilk.selectedIndex];

        const tempOpt = (typeId === TYPE_TEMP && cfgTemp) ? cfgTemp.options[cfgTemp.selectedIndex] : null;
        const syrupOpt = (typeId === TYPE_SYRUP && cfgSyrup) ? cfgSyrup.options[cfgSyrup.selectedIndex] : null;

        const sizeDelta = getSelectedDelta(cfgSize);
        const milkDelta = getSelectedDelta(cfgMilk);

        // Precio unitario base
        let unitPrice = Number(selectedProduct.basePrice) + sizeDelta + milkDelta;

        // Jarabe puede sumar precio (si tus options lo traen en data-delta)
        if (typeId === TYPE_SYRUP) {
            unitPrice += getSelectedDelta(cfgSyrup);
        }

        // Crema batida (si tiene costo fijo; si no, WHIPPED_CREAM_PRICE = 0)
        if (typeId === TYPE_WHIP && hasWhippedCream) {
            unitPrice += WHIPPED_CREAM_PRICE;
        }

        addToCart({
            productId: selectedProduct.productId,
            barCode: selectedProduct.barCode,
            productTypeId: typeId,
            name: selectedProduct.name,
            basePrice: Number(selectedProduct.basePrice),

            sizeId,
            sizeName: sizeOpt?.textContent?.split(' (')[0] || 'Size',
            sizeDelta,

            milkTypeId,
            milkName: milkOpt?.textContent?.split(' (')[0] || 'Milk',
            milkDelta,

            // Extras guardados para mostrar y para checkout
            temperatureId,
            tempName: tempOpt ? (tempOpt.textContent || 'Temp') : 'N/A',

            syrupId,
            syrupName: syrupOpt ? (syrupOpt.textContent || 'Syrup') : 'N/A',

            hasWhippedCream,

            unitPrice,
            qty
        });

        configModal.hide();
        selectedProduct = null;
    });

    // --------------------------
    // Cart actions
    // --------------------------
    btnClearCart.addEventListener('click', () => {
        cart = [];
        renderCart();
    });

    cartBody.addEventListener('click', (e) => {
        const btn = e.target.closest('button');
        if (!btn) return;

        const idx = Number(btn.getAttribute('data-idx'));
        const action = btn.getAttribute('data-action');
        if (Number.isNaN(idx) || !action) return;

        if (action === 'inc') cart[idx].qty += 1;
        if (action === 'dec') cart[idx].qty = Math.max(1, cart[idx].qty - 1);
        if (action === 'remove') cart.splice(idx, 1);

        renderCart();
    });

    // --------------------------
    // Payment + Checkout
    // --------------------------
    function resetPayUI() {
        payError.classList.add('d-none');
        payError.textContent = '';
        paySuccess.classList.add('d-none');
        paySuccess.textContent = '';

        cashSection.classList.add('d-none');
        cardSection.classList.add('d-none');

        cashReceived.value = '';
        cashChange.textContent = money(0);

        cardSuccess.classList.add('d-none');
        cardProcessing.classList.remove('d-none');

        // deshabilitar si no hay carrito
        const hasItems = cart.length > 0;
        payCash.disabled = !hasItems;
        payCard.disabled = !hasItems;
        btnCashConfirm.disabled = !hasItems;
    }

    payModalEl.addEventListener('show.bs.modal', () => {
        resetPayUI();
    });

    function setPayError(msg) {
        payError.classList.remove('d-none');
        payError.textContent = msg;
    }

    function setPaySuccess(msg) {
        paySuccess.classList.remove('d-none');
        paySuccess.textContent = msg;
    }

    async function doCheckout(paidMethodId) {
        // paidMethodId: AJUSTA a tus IDs reales de PaidMethods
        const payload = {
            isDelivery: !!isDelivery.checked,
            paidMethodId: paidMethodId,
            lines: cart.map(i => ({
                productId: i.productId,
                quantity: i.qty,
                sizeId: i.sizeId,
                milkTypeId: i.milkTypeId,
                temperatureId: i.temperatureId,
                syrupId: i.syrupId,
                hasWhippedCream: i.hasWhippedCream
            }))
        };

        const result = await httpPostJson('/Pos/Checkout', payload);
        return result; // { orderId, subtotal, totalAmount }
    }

    payCash.addEventListener('click', () => {
        resetPayUI();
        cashSection.classList.remove('d-none');
    });

    cashReceived.addEventListener('input', () => {
        const { total } = calcTotals();
        const received = Number(cashReceived.value || 0);
        const change = Math.max(0, received - total);
        cashChange.textContent = money(change);
    });

    btnCashConfirm.addEventListener('click', async () => {
        payError.classList.add('d-none');
        paySuccess.classList.add('d-none');

        const { total } = calcTotals();
        const received = Number(cashReceived.value || 0);

        if (received < total) {
            setPayError("El efectivo recibido es menor al total.");
            return;
        }

        try {
            // Asumimos 1 = Efectivo
            const res = await doCheckout(1);

            setPaySuccess(`Venta registrada. OrderId: ${res.orderId}`);
            cart = [];
            renderCart();
        } catch (e) {
            setPayError("No se pudo registrar la venta. Revisa /Pos/Checkout y tu backend.");
        }
    });

    payCard.addEventListener('click', async () => {
        resetPayUI();
        cardSection.classList.remove('d-none');

        // Simulación: 4 segundos de "cargando"
        setTimeout(async () => {
            try {
                // Asumimos 2 = Tarjeta
                const res = await doCheckout(2);

                cardProcessing.classList.add('d-none');
                cardSuccess.classList.remove('d-none');
                setPaySuccess(`Venta registrada. OrderId: ${res.orderId}`);

                cart = [];
                renderCart();
            } catch (e) {
                cardProcessing.classList.add('d-none');
                setPayError("Pago simulado OK, pero falló el registro de la venta en backend.");
            }
        }, 4000);
    });

    // Init
    renderCart();
})();
