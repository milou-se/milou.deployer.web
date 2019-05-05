class DeploymentTarget {
    constructor(
    targetId,
    name,
    url,
    editUrl,
    historyUrl,
    statusKey,
    statusDisplayName,
    statusUrl,
    isPreReleaseVersion,
    semanticVersion,
    preReleaseClass,
    intervalAgo,
    intervalAgoName,
    deployedAtLocalTime,
    environmentType,
    metadataUrl) {
        this.targetId = targetId;
        this.name = name;
        this.url = url;
        this.editUrl = editUrl;
        this.historyUrl = historyUrl;
        this.statusKey = statusKey;
        this.statusDisplayName = statusDisplayName;
        this.statusUrl = statusUrl;
        this.isPreReleaseVersion = isPreReleaseVersion;
        this.semanticVersion = semanticVersion;
        this.preReleaseClass = preReleaseClass;
        this.intervalAgo = intervalAgo;
        this.intervalAgoName = intervalAgoName;
        this.deployedAtLocalTime = deployedAtLocalTime;
        this.environmentType = environmentType;
        this.metadataUrl = metadataUrl;
    }

    static from(json) {
        return Object.assign(new DeploymentTarget(), json);
    }
}

class TargetStatus {
    constructor(
    key,
    displayName,
    semanticVersion,
    isPreReleaseVersion,
    preReleaseClass,
    intervalAgo,
    intervalAgoName,
    deployedAtLocalTime) {
        this.key = key;
        this.displayName = displayName;
        this.isPreReleaseVersion = isPreReleaseVersion;
        this.semanticVersion = semanticVersion;
        this.preReleaseClass = preReleaseClass;
        this.intervalAgo = intervalAgo;
        this.intervalAgoName = intervalAgoName;
        this.deployedAtLocalTime = deployedAtLocalTime;
    }

    static from(json) {
        return Object.assign(new TargetStatus(), json);
    }
}

async function getTargetStatus(target) {

    const response = await fetch(target.statusUrl);

    console.dir(response);

    const json = await response.json();

    const targetStatus = TargetStatus.from(json);

    return targetStatus;
}

async function getTargets() {

    const response = await fetch("/api/targets");

    console.dir(response);

    const json = await response.json();

    const targets = json.targets.map(target => DeploymentTarget.from(target));

    return targets;
}

let app = {};
let targets = [];

async function buildApp() {
    targets = await getTargets();
    app = new Vue({
        el: "#app",
        data: {
            targets: targets
        }
    });

    targets.forEach(target => {

        if (!target.url) {
            return;
        }

        getTargetStatus(target).then(status => {
            target.statusKey = `deploy-status-${status.key}`;
            target.statusDisplayName = status.displayName;
            target.isPreReleaseVersion = status.isPreReleaseVersion;
            target.semanticVersion = status.semanticVersion;
            target.preReleaseClass = status.preReleaseClass;
            target.intervalAgo = status.intervalAgo;
            target.intervalAgoName = status.intervalAgoName;
            target.deployedAtLocalTime = status.deployedAtLocalTime;
        });
    });
}

buildApp();