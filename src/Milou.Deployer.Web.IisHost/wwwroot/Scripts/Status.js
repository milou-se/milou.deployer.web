class DeploymentTarget {
    constructor(targetId, name, url, editUrl, historyUrl, statusKey, statusDisplayName, statusUrl) {
        this.targetId = targetId;
        this.name = name;
        this.url = url;
        this.editUrl = editUrl;
        this.historyUrl = historyUrl;
        this.statusKey = statusKey;
        this.statusDisplayName = statusDisplayName;
        this.statusUrl = statusUrl;
    }
    static from(json) {
        return Object.assign(new DeploymentTarget(), json);
    }
}

class TargetStatus {
    constructor(key, displayName) {
        this.key = key;
        this.displayName = displayName;
    }
    static from(json) {
        return Object.assign(new TargetStatus(), json);
    }
}

async function getTargetStatus(target) {

    let response = await fetch(target.statusUrl);

    console.dir(response);

    let json = await response.json();

    let targetStatus = TargetStatus.from(json);

    return targetStatus;
}

async function getTargets() {

    let response = await fetch('/api/targets');

    console.dir(response);

    let json = await response.json();

    let targets = json.targets.map(target => DeploymentTarget.from(target));

    return targets;
}

let app = {};
let targets = [];

async function buildApp() {
    targets = await getTargets();
    app = new Vue({
        el: '#app',
        data: {
            targets: targets
        }
    });

    targets.forEach(target => {
        getTargetStatus(target).then(status => {
            target.statusKey = 'deploy-status-' + status.key;
            target.statusDisplayName = status.displayName;
        });
    });
}

buildApp();
