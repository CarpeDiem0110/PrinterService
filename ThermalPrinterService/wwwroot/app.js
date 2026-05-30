const statusElements = {
    serviceState: document.getElementById("serviceState"),
    connected: document.getElementById("connectedValue"),
    mode: document.getElementById("modeValue"),
    paper: document.getElementById("paperValue"),
    cover: document.getElementById("coverValue"),
    temperature: document.getElementById("temperatureValue"),
    activeErrors: document.getElementById("activeErrors"),
    lastJob: document.getElementById("lastJobValue"),
    queue: document.getElementById("queueSummary"),
    logsOutput: document.getElementById("logsOutput"),
    toast: document.getElementById("toast")
};

document.getElementById("connectBtn").addEventListener("click", connectPrinter);
document.getElementById("printTextBtn").addEventListener("click", printText);
document.getElementById("printImageBtn").addEventListener("click", printImage);
document.getElementById("simulateErrorBtn").addEventListener("click", simulateError);
document.getElementById("clearErrorBtn").addEventListener("click", clearError);
document.getElementById("getLogsBtn").addEventListener("click", loadLogs);
document.getElementById("exportCsvBtn").addEventListener("click", () => {
    window.location.href = "/logs/export";
});
document.getElementById("exportJsonBtn").addEventListener("click", exportJson);
document.getElementById("reprintBtn").addEventListener("click", reprintFailedJob);

refreshStatus();

async function connectPrinter() {
    const mode = document.querySelector("input[name='mode']:checked").value;

    await postJson("/connect", { mode });
    showToast(`Connected via ${mode}`);
    await refreshStatus();
}

async function refreshStatus() {
    const status = await requestJson("/status");
    renderStatus(status);
}

async function printText() {
    const text = document.getElementById("printTextInput").value.trim();

    try {
        await postJson("/print/text", { text });
        showToast("Text print request completed");
    } finally {
        await refreshDashboard();
    }
}

async function printImage() {
    const input = document.getElementById("imageInput");

    if (!input.files.length) {
        showToast("Select an image first", true);
        return;
    }

    const formData = new FormData();
    formData.append("image", input.files[0]);

    try {
        await requestJson("/print/image", {
            method: "POST",
            body: formData
        });

        showToast("Image upload completed");
    } finally {
        await refreshDashboard();
    }
}

async function simulateError() {
    const errorCode = [...document.querySelectorAll("#errorOptions input:checked")]
        .map(input => input.value);

    if (!errorCode.length) {
        showToast("Select at least one error", true);
        return;
    }

    await postJson("/simulate-error", { errorCode });
    showToast("Errors simulated");
    await refreshStatus();
}

async function clearError() {
    await requestJson("/clear-error", { method: "POST" });
    document.querySelectorAll("#errorOptions input").forEach(input => {
        input.checked = false;
    });

    showToast("Errors cleared");
    await refreshStatus();
}

async function loadLogs() {
    const logs = await requestJson("/logs");
    statusElements.logsOutput.textContent = JSON.stringify(logs, null, 2);
}

async function exportJson() {
    const logs = await requestJson("/logs");
    const blob = new Blob([JSON.stringify(logs, null, 2)], {
        type: "application/json"
    });

    downloadBlob(blob, `printer-logs-${formatFileDate(new Date())}.json`);
}

async function reprintFailedJob() {
    try {
        await requestJson("/reprint", { method: "POST" });
        showToast("Reprint request completed");
    } finally {
        await refreshDashboard();
    }
}

async function refreshDashboard() {
    await Promise.allSettled([
        refreshStatus(),
        loadLogs()
    ]);
}

function renderStatus(status) {
    statusElements.serviceState.textContent = status.service ?? "Thermal Printer Service";
    statusElements.connected.textContent = status.connection?.connected ? "Yes" : "No";
    statusElements.mode.textContent = status.connection?.mode ?? "-";
    statusElements.paper.textContent = status.health?.paper ?? "-";
    statusElements.cover.textContent = status.health?.cover ?? "-";
    statusElements.temperature.textContent = status.health?.temperature ?? "-";

    renderErrors(status.health?.currentErrors ?? []);
    statusElements.lastJob.textContent = status.lastJob
        ? JSON.stringify(status.lastJob, null, 2)
        : "-";
    renderQueue(status.queue);
}

function renderErrors(errors) {
    statusElements.activeErrors.innerHTML = "";

    if (!errors.length) {
        const chip = document.createElement("span");
        chip.className = "chip ok";
        chip.textContent = "No active errors";
        statusElements.activeErrors.appendChild(chip);
        return;
    }

    for (const error of errors) {
        const chip = document.createElement("span");
        chip.className = "chip";
        chip.textContent = `${error.code}: ${error.detail}`;
        statusElements.activeErrors.appendChild(chip);
    }
}

function renderQueue(queue) {
    const items = [
        ["Pending", queue?.pending ?? 0],
        ["Active", queue?.active ?? 0],
        ["Completed", queue?.completed ?? 0],
        ["Failed", queue?.failed ?? 0]
    ];

    statusElements.queue.innerHTML = items
        .map(([label, value]) => `<div class="queue-item"><span>${label}</span><strong>${value}</strong></div>`)
        .join("");
}

async function postJson(url, body) {
    return requestJson(url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(body)
    });
}

async function requestJson(url, options = {}) {
    const response = await fetch(url, options);
    const contentType = response.headers.get("content-type") ?? "";
    const payload = contentType.includes("application/json")
        ? await response.json()
        : await response.text();

    if (!response.ok) {
        const message = payload?.detail || payload?.title || response.statusText;
        showToast(message, true);
        throw new Error(message);
    }

    return payload;
}

function downloadBlob(blob, fileName) {
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");

    link.href = url;
    link.download = fileName;
    link.click();

    URL.revokeObjectURL(url);
}

function formatFileDate(date) {
    const pad = value => String(value).padStart(2, "0");

    return [
        date.getFullYear(),
        pad(date.getMonth() + 1),
        pad(date.getDate()),
        pad(date.getHours()),
        pad(date.getMinutes()),
        pad(date.getSeconds())
    ].join("");
}

function showToast(message, isError = false) {
    statusElements.toast.textContent = message;
    statusElements.toast.classList.toggle("error", isError);
    statusElements.toast.classList.add("show");

    window.clearTimeout(showToast.timeoutId);
    showToast.timeoutId = window.setTimeout(() => {
        statusElements.toast.classList.remove("show");
    }, 2600);
}
