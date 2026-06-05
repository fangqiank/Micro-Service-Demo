/* views.js -- DOM Rendering Functions */

function createElement(tag, attrs = {}, children = []) {
    const el = document.createElement(tag);
    for (const [key, val] of Object.entries(attrs)) {
        if (key === 'className') { el.className = val; }
        else if (key === 'textContent') { el.textContent = val; }
        else if (key.startsWith('on')) { el.addEventListener(key.slice(2).toLowerCase(), val); }
        else { el.setAttribute(key, val); }
    }
    for (const child of children) {
        if (typeof child === 'string') { el.appendChild(document.createTextNode(child)); }
        else if (child) { el.appendChild(child); }
    }
    return el;
}

function renderPlatformCard(platform, onViewCommands) {
    const card = createElement('div', { className: 'card' }, [
        createElement('div', { className: 'card-title', textContent: platform.name }),
        createElement('div', { className: 'card-meta' }, [
            createElement('span', { className: 'card-badge', textContent: platform.publisher }),
            createElement('span', { className: 'card-badge', textContent: platform.cost })
        ]),
        createElement('div', { className: 'card-actions' }, [
            createElement('button', {
                className: 'btn btn-link btn-sm',
                textContent: 'View Commands',
                onClick: () => onViewCommands(platform)
            })
        ])
    ]);
    return card;
}

function renderPlatformList(platforms, container, onViewCommands) {
    container.innerHTML = '';
    if (platforms.length === 0) return false;
    const grid = createElement('div', { className: 'card-grid' });
    platforms.forEach(p => grid.appendChild(renderPlatformCard(p, onViewCommands)));
    container.appendChild(grid);
    return true;
}

function renderCommandTable(commands, container) {
    container.innerHTML = '';
    if (commands.length === 0) return false;

    const table = createElement('table', { className: 'command-table' });
    const thead = createElement('thead');
    const headerRow = createElement('tr');
    ['ID', 'How To', 'Command Line', 'Platform ID'].forEach(text => {
        headerRow.appendChild(createElement('th', { textContent: text }));
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    const tbody = createElement('tbody');
    commands.forEach(cmd => {
        const row = createElement('tr');
        row.appendChild(createElement('td', { textContent: cmd.id }));
        row.appendChild(createElement('td', { textContent: cmd.howTo }));
        row.appendChild(createElement('td', {}, [
            createElement('span', { className: 'cmd-code', textContent: cmd.commandLine })
        ]));
        row.appendChild(createElement('td', { textContent: cmd.platformId }));
        tbody.appendChild(row);
    });
    table.appendChild(tbody);
    container.appendChild(table);
    return true;
}

function renderPlatformOptions(platforms, selectElement, selectedId) {
    selectElement.innerHTML = '<option value="">-- Select a platform --</option>';
    platforms.forEach(p => {
        const opt = createElement('option', { value: p.id, textContent: `[${p.id}] ${p.name}` });
        if (String(p.id) === String(selectedId)) opt.selected = true;
        selectElement.appendChild(opt);
    });
}

function renderStatusCard(serviceKey, result, container) {
    const dot = document.getElementById(`dot-${serviceKey}`);
    const detail = document.getElementById(`detail-${serviceKey}`);
    const time = document.getElementById(`time-${serviceKey}`);

    if (!dot) return;

    if (result.ok) {
        dot.className = 'status-dot status-ok';
        const count = Array.isArray(result.data) ? result.data.length : '?';
        detail.textContent = `Online -- ${count} platform(s)`;
        detail.style.color = 'var(--color-green)';
    } else {
        dot.className = 'status-dot status-error';
        detail.textContent = `Offline -- ${result.error}`;
        detail.style.color = 'var(--color-red)';
    }

    if (result.elapsed != null) {
        time.textContent = `Response time: ${result.elapsed}ms`;
    }
}

function showNotification(message, type = 'info') {
    const container = document.getElementById('toast-container');
    const toast = createElement('div', { className: `toast toast-${type}`, textContent: message });
    container.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('toast-exit');
        setTimeout(() => toast.remove(), 200);
    }, 3000);
}

function show(el) { el.classList.remove('hidden'); }
function hide(el) { el.classList.add('hidden'); }
