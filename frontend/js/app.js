/* app.js -- Main Application Controller */

(function () {
    'use strict';

    /* === DOM References === */
    const $ = id => document.getElementById(id);

    const tabBtns = document.querySelectorAll('.tab-btn');
    const tabContents = document.querySelectorAll('.tab-content');

    const platformFormPanel = $('platform-form-panel');
    const platformForm = $('platform-form');
    const platformList = $('platform-list');
    const platformLoading = $('platform-loading');
    const platformEmpty = $('platform-empty');
    const platformError = $('platform-error');

    const commandFormPanel = $('command-form-panel');
    const commandForm = $('command-form');
    const commandList = $('command-list');
    const commandLoading = $('command-loading');
    const commandEmpty = $('command-empty');
    const commandError = $('command-error');
    const platformSelect = $('cmd-platform-select');

    let selectedPlatformId = null;

    /* === Tab Switching === */
    function switchTab(tabName) {
        tabBtns.forEach(btn => btn.classList.toggle('active', btn.dataset.tab === tabName));
        tabContents.forEach(sec => sec.classList.toggle('active', sec.id === `tab-${tabName}`));

        if (tabName === 'platforms') loadPlatforms();
        if (tabName === 'commands') loadCommandPlatforms();
        if (tabName === 'status') checkStatus();
    }

    tabBtns.forEach(btn => {
        btn.addEventListener('click', () => switchTab(btn.dataset.tab));
    });

    /* === Platforms === */
    async function loadPlatforms() {
        hide(platformEmpty);
        hide(platformError);
        show(platformLoading);
        platformList.innerHTML = '';

        const result = await PlatformAPI.getAll();
        hide(platformLoading);

        if (!result.ok) {
            show(platformError);
            platformError.textContent = `Failed to load platforms: ${result.error}`;
            return;
        }

        if (!renderPlatformList(result.data, platformList, p => navigateToCommands(p.id))) {
            show(platformEmpty);
        }
    }

    function navigateToCommands(platformId) {
        selectedPlatformId = platformId;
        switchTab('commands');
        showNotification('Platform data syncs via gRPC/RabbitMQ at startup. Commands require K8S or local RabbitMQ.', 'info');
    }

    /* Add Platform Form */
    $('btn-add-platform').addEventListener('click', () => {
        platformFormPanel.classList.toggle('hidden');
        if (!platformFormPanel.classList.contains('hidden')) {
            $('pf-name').focus();
        }
    });

    $('btn-cancel-platform').addEventListener('click', () => {
        hide(platformFormPanel);
        platformForm.reset();
    });

    platformForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            name: $('pf-name').value.trim(),
            publisher: $('pf-publisher').value.trim(),
            cost: $('pf-cost').value.trim()
        };

        if (!data.name || !data.publisher || !data.cost) {
            showNotification('All fields are required', 'error');
            return;
        }

        const result = await PlatformAPI.create(data);
        if (result.ok) {
            showNotification(`Platform "${data.name}" created`, 'success');
            platformForm.reset();
            hide(platformFormPanel);
            loadPlatforms();
        } else {
            showNotification(`Failed: ${result.error}`, 'error');
        }
    });

    /* === Commands === */
    async function loadCommandPlatforms() {
        const result = await CommandAPI.getAllPlatforms();
        if (result.ok) {
            renderPlatformOptions(result.data, platformSelect, selectedPlatformId);
            if (result.data.length === 0) {
                show(commandEmpty);
                commandEmpty.querySelector('p').innerHTML =
                    'No platforms in CommandService.<br>' +
                    '<small>Platforms are synced from PlatformService via gRPC/RabbitMQ. ' +
                    'Start RabbitMQ locally or deploy to K8S to enable sync.</small>';
                $('btn-add-command').disabled = true;
                return;
            }
            if (selectedPlatformId) {
                $('btn-add-command').disabled = false;
                loadCommands(selectedPlatformId);
            }
        } else {
            renderPlatformOptions([], platformSelect);
            show(commandError);
            commandError.textContent = `Cannot load platforms from CommandService: ${result.error}`;
        }
    }

    platformSelect.addEventListener('change', () => {
        const val = platformSelect.value;
        selectedPlatformId = val ? Number(val) : null;
        $('btn-add-command').disabled = !selectedPlatformId;
        if (selectedPlatformId) {
            loadCommands(selectedPlatformId);
        } else {
            commandList.innerHTML = '';
            hide(commandEmpty);
        }
    });

    async function loadCommands(platformId) {
        hide(commandEmpty);
        hide(commandError);
        show(commandLoading);
        commandList.innerHTML = '';

        const result = await CommandAPI.getCommands(platformId);
        hide(commandLoading);

        if (!result.ok) {
            if (result.status === 404) {
                show(commandEmpty);
                commandEmpty.querySelector('p').textContent = 'Platform not found in CommandService. Platforms are synced via gRPC/RabbitMQ at startup.';
            } else {
                show(commandError);
                commandError.textContent = `Failed to load commands: ${result.error}`;
            }
            return;
        }

        if (!renderCommandTable(result.data, commandList)) {
            show(commandEmpty);
            commandEmpty.querySelector('p').textContent = 'No commands found for this platform.';
        }
    }

    /* Add Command Form */
    $('btn-add-command').addEventListener('click', () => {
        if (!selectedPlatformId) return;
        show(commandFormPanel);
        $('cf-howto').focus();
    });

    $('btn-cancel-command').addEventListener('click', () => {
        hide(commandFormPanel);
        commandForm.reset();
    });

    commandForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const data = {
            howTo: $('cf-howto').value.trim(),
            commandLine: $('cf-cmdline').value.trim()
        };

        if (!data.howTo || !data.commandLine) {
            showNotification('All fields are required', 'error');
            return;
        }

        const result = await CommandAPI.createCommand(selectedPlatformId, data);
        if (result.ok) {
            showNotification('Command created', 'success');
            commandForm.reset();
            hide(commandFormPanel);
            loadCommands(selectedPlatformId);
        } else {
            showNotification(`Failed: ${result.error}`, 'error');
        }
    });

    /* === System Status === */
    async function checkStatus() {
        const services = [
            { key: 'platform', fn: () => PlatformAPI.getAll() },
            { key: 'command', fn: () => CommandAPI.getAllPlatforms() }
        ];

        for (const svc of services) {
            const start = performance.now();
            const result = await svc.fn();
            result.elapsed = Math.round(performance.now() - start);
            renderStatusCard(svc.key, result);
        }
    }

    $('btn-check-status').addEventListener('click', checkStatus);

    $('btn-test-inbound').addEventListener('click', async () => {
        const resultEl = $('inbound-result');
        resultEl.classList.remove('hidden');
        resultEl.textContent = 'Testing...';

        const result = await CommandAPI.testConnection();
        if (result.ok) {
            resultEl.textContent = `Success: ${result.data}`;
            resultEl.style.color = 'var(--color-green)';
        } else {
            resultEl.textContent = `Failed: ${result.error}`;
            resultEl.style.color = 'var(--color-red)';
        }
    });

    /* === Init === */
    switchTab('platforms');
})();
