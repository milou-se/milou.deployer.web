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
    metadataUrl,
    statusMessage,
    latestNewerAvailable,
    deployEnabled,
    packages,
    selectedPackage) {
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
        this.statusMessage = statusMessage;
        this.latestNewerAvailable = latestNewerAvailable;
        this.deployEnabled = deployEnabled;
        this.packages = packages;
        this.selectedPackage = selectedPackage || -1;
    }

    get statusTitle() {
        if (this.latestNewerAvailable) {
            return this.latestNewerAvailable;
        }

        if (this.statusMessage) {
            return this.statusMessage;
        }

        return "";
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
    deployedAtLocalTime,
    statusMessage,
    latestNewerAvailable,
    deployEnabled,
    packages,
    selectedPackageIndex) {
        this.key = key;
        this.displayName = displayName;
        this.isPreReleaseVersion = isPreReleaseVersion;
        this.semanticVersion = semanticVersion;
        this.preReleaseClass = preReleaseClass;
        this.intervalAgo = intervalAgo;
        this.intervalAgoName = intervalAgoName;
        this.deployedAtLocalTime = deployedAtLocalTime;
        this.statusMessage = statusMessage;
        this.latestNewerAvailable = latestNewerAvailable;
        this.deployEnabled = deployEnabled;
        this.packages = packages;
        this.selectedPackageIndex = selectedPackageIndex;
    }

    static from(json) {
        return Object.assign(new TargetStatus(), json);
    }
}

async function getTargetStatus(target) {

    const response = await fetch(target.statusUrl);

    //console.dir(response);

    const json = await response.json();

    const targetStatus = TargetStatus.from(json);

    return targetStatus;
}

async function getTargets() {

    const response = await fetch("/api/targets");

    //console.dir(response);

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
        },
        methods: {
            deployPackageVersion: function (event) {

                //let button = event.target;

                //let formElement = button.form;

                //let packageVersionElement = formElement.querySelector(".packageVersionSelect");


            }
        },
        computed: {

            reversedMessage: function() {

                return "";
            }
        }
    });

    targets.forEach(target => {

        if (!target.url) {
            return;
        }

        getTargetStatus(target).then(status => {
            target.statusKey = status.key;
            target.statusDisplayName = status.displayName;
            target.isPreReleaseVersion = status.isPreReleaseVersion;
            target.semanticVersion = status.semanticVersion;
            target.preReleaseClass = status.preReleaseClass;
            target.intervalAgo = status.intervalAgo;
            target.intervalAgoName = status.intervalAgoName;
            target.deployedAtLocalTime = status.deployedAtLocalTime;
            target.statusMessage = status.statusMessage;
            target.latestNewerAvailable = status.latestNewerAvailable;
            target.deployEnabled = status.deployEnabled;
            target.packages = status.packages;

            if (target.packages && target.packages.length > 0) {
                if (status.selectedPackageIndex >= 0) {
                    target.selectedPackage = status.selectedPackageIndex;
                } else {
                    target.selectedPackage = 0;
                }
            }
        });
    });
}

buildApp();