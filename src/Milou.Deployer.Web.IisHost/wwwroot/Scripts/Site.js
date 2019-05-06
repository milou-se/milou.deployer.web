class SemanticVersion {
    constructor(normalized) {
        this.normalized = normalized;

        this.major = parseInt("1");
        this.minor = parseInt("0");
        this.patch = parseInt("0");
    }

    static parse(normalized) {

        if (!normalized) {
            return null;
        }

        return new SemanticVersion(normalized);
    }

    static compare(left, right) {

        if (right.major > left.major) {
            return 1;
        }

        if (right.major < left.major) {
            return -1;
        }

        if (right.minor > left.minor) {
            return 1;
        }

        if (right.minor < left.minor) {
            return -1;
        }

        if (right.patch > left.patch) {
            return 1;
        }

        if (right.patch < left.patch) {
            return -1;
        }

        return 0;
    }
}

function createSpanLogItemElement(data) {

    const eventData = JSON.parse(data);

    return createSpanLogItemElementFromJson(eventData);
}

function createSpanLogItemElementFromJson(jsonData) {

    const logElement = document.createElement("span");

    const eventData = jsonData;

    if (eventData.Message) {
        logElement.innerHTML = eventData.Message;
    } else {
        logElement.innerHTML = `<span class="timestamp">${eventData.FormattedTimestamp}</span>
<span class="level-${eventData.Level}">[${eventData.Level}]</span>
<span class="message">${eventData.RenderedTemplate}</span><br />`;
    }

    return logElement;
}

function parseLogLines(jsonLogs) {

    const lines = JSON.parse(jsonLogs);

    const logElements = document.createElement("div");

    lines.items.forEach(function(element) {

        const spanElement = createSpanLogItemElementFromJson(element);

        logElements.appendChild(spanElement);
    });

    return logElements;
}

$(function() {

    $("div.projects").hide();
    $("div.targets").hide();

    $(".organization-toggler").click(function() {
        $(this).next("div.projects").toggle();
    });

    $(".project-toggler").click(function() {
        $(this).next("div.targets").toggle();
    });

    $(".deploy-button").closest("form").submit(function(e) {

        const packageVersion = $(this).find('select[name="packageVersion"]').val();
        var targetId = $(this).find('input[name="targetId"]').val();

        if (!targetId) {
            targetId = $(this).find('select[name="targetId"]').val();
        }

        e.preventDefault();

        var confirmMessage = `Deploy ${packageVersion} to ${targetId}`;

        const selectedVersion = $(this).find('select[name="packageVersion"]').find("option:selected");
        const currentVersionMajor = $(this).find('input[name="current-version-major"]').val();
        const currentVersionMinor = $(this).find('input[name="current-version-minor"]').val();
        const currentVersionPatch = $(this).find('input[name="current-version-patch"]').val();
        const currentVersionIsPreRelease = Boolean($(this).find('input[name="current-version-isPreRelease"]').val());

        if (selectedVersion && currentVersionMajor && currentVersionMinor && currentVersionPatch) {
            const selectedVersionMajor = parseInt(selectedVersion.attr("data-version-major"));
            const selectedVersionMinor = parseInt(selectedVersion.attr("data-version-minor"));
            const selectedVersionPatch = parseInt(selectedVersion.attr("data-version-patch"));
            const selectedVersionIsPreRelease = Boolean(selectedVersion.attr("data-version-isPreRelease"));

            const currentSemanticVersion = {
                major: currentVersionMajor,
                minor: currentVersionMinor,
                patch: currentVersionPatch
            };

            const selectedSemanticVersion = {
                major: parseInt(selectedVersionMajor),
                minor: parseInt(selectedVersionMinor),
                patch: parseInt(selectedVersionPatch)
            };

            const compareValue = compareSemVer(currentSemanticVersion, selectedSemanticVersion);

            if (compareValue === -1) {
                confirmMessage += " WARNING! older version";
            }

            if (!currentVersionIsPreRelease && !selectedVersionIsPreRelease) {

                if (compareValue === 0) {
                    confirmMessage += " WARNING! same version already deployed";
                }
            }

            if (!currentVersionIsPreRelease && selectedVersionIsPreRelease) {
                confirmMessage += " WARNING! selected version is pre-release";
            }
        }

        const confirmed = confirm(confirmMessage);

        if (confirmed === true) {
            this.submit();
        } else {
            console.log("aborted");
        }

    });

    function compareSemVer(original, newer) {

        if (newer.major > original.major) {
            return 1;
        }

        if (newer.major < original.major) {
            return -1;
        }

        if (newer.minor > original.minor) {
            return 1;
        }

        if (newer.minor < original.minor) {
            return -1;
        }

        if (newer.patch > original.patch) {
            return 1;
        }

        if (newer.patch < original.patch) {
            return -1;
        }

        return 0;
    }
});