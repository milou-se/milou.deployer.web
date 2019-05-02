class DeploymentTarget {
    constructor(targetId, name, url, editUrl, historyUrl) {
        this.targetId = targetId;
        this.name = name;
        this.url = url;
        this.editUrl = editUrl;
        this.historyUrl = historyUrl;
    }
    static from(json) {
        return Object.assign(new DeploymentTarget(), json);
    }
}

async function getTargets() {

    let response = await fetch('/api/targets');

    console.dir(response);

    let json = await response.json();

    let targets = json.targets.map(target => DeploymentTarget.from(target));

    return targets;
}

let app = {};

async function buildApp() {
    let targets = await getTargets();
    app = new Vue({
        el: '#app',
        data: {
            targets: targets
        }
    });
}

buildApp();
