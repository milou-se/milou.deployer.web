function createSpanLogItemElement(data) {

    const eventData = JSON.parse(data);

    return createSpanLogItemElementFromJson(eventData);
}

function createSpanLogItemElementFromJson(jsonData) {

    const logElement = document.createElement("span");

    const eventData = jsonData;

    try {
        if (eventData.Message) {
            logElement.innerHTML = eventData.Message;
        } else {
            logElement.innerHTML = `<span class="timestamp">${eventData.FormattedTimestamp}</span>
<span class="level-${eventData.Level}">[${eventData.Level}]</span>
<span class="message">${eventData.RenderedTemplate}</span><br />`;
        }
    } catch (ex) {
        console.debug(ex);
        logElement.innerText = jsonData;
    }

    return logElement;
}

function parseLogLines(jsonLogs) {
    const lines = JSON.parse(jsonLogs);
    showLogLines(lines.items);
}

function showLogLines(lines) {

    if (!lines) {
        console.debug("Lines are not defined");
    }

    const logElements = document.createElement("div");

    lines.forEach(function(element) {

        const spanElement = createSpanLogItemElementFromJson(JSON.parse(element));

        logElements.appendChild(spanElement);
    });

    return logElements;
}

$(function() {

    $("div.projects").hide();
    $("div.targets").hide();
    $("div.tab").click(function () {
        let id = $(this).attr("id");

        if (id) {
            let currentTab = $("#tab-content-" + id);
            currentTab.show();
            currentTab.siblings().hide();
        }
    });

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