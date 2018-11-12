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

        var spanElement = createSpanLogItemElementFromJson(element);

        logElements.appendChild(spanElement);
    });

    return logElements;
}

$(function () {

    $("div.projects").hide();
    $("div.targets").hide();

    $('.organization-toggler').click(function () {
        $(this).next('div.projects').toggle();
    });

    $('.project-toggler').click(function () {
        $(this).next('div.targets').toggle();
    });

    $('.deploy-button').closest('form').submit(function (e) {

        var packageVersion = $(this).find('select[name="packageVersion"]').val();
        var targetId = $(this).find('input[name="targetId"]').val();

        if (!targetId) {
            targetId = $(this).find('select[name="targetId"]').val();
        }

        e.preventDefault();

        var confirmMessage = 'Deploy ' + packageVersion + ' to ' + targetId;

        var selectedVersion = $(this).find('select[name="packageVersion"]').find('option:selected');
        var currentVersionMajor = $(this).find('input[name="current-version-major"]').val();
        var currentVersionMinor = $(this).find('input[name="current-version-minor"]').val();
        var currentVersionPatch = $(this).find('input[name="current-version-patch"]').val();
        var currentVersionIsPreRelease = Boolean($(this).find('input[name="current-version-isPreRelease"]').val());

        if (selectedVersion && currentVersionMajor && currentVersionMinor && currentVersionPatch) {
            var selectedVersionMajor = parseInt(selectedVersion.attr('data-version-major'));
            var selectedVersionMinor = parseInt(selectedVersion.attr('data-version-minor'));
            var selectedVersionPatch = parseInt(selectedVersion.attr('data-version-patch'));
            var selectedVersionIsPreRelease = Boolean(selectedVersion.attr('data-version-isPreRelease'));

            var currentSemanticVersion = {
                major: currentVersionMajor,
                minor: currentVersionMinor,
                patch: currentVersionPatch
            }

            var selectedSemanticVersion = {
                major: parseInt(selectedVersionMajor),
                minor: parseInt(selectedVersionMinor),
                patch: parseInt(selectedVersionPatch)
            }

            var compareValue = compareSemVer(currentSemanticVersion, selectedSemanticVersion);

            if (compareValue === -1) {
                confirmMessage += ' WARNING! older version';
            }

            if (!currentVersionIsPreRelease && !selectedVersionIsPreRelease) {

                if (compareValue === 0) {
                    confirmMessage += ' WARNING! same version already deployed';
                }
            }

            if (!currentVersionIsPreRelease && selectedVersionIsPreRelease) {
                confirmMessage += ' WARNING! selected version is pre-release';
            }
        }

        var confirmed = confirm(confirmMessage);

        if (confirmed === true) {
            this.submit();
        } else {
            console.log('aborted');
        }

    });

    function compareSemVer(orignal, newer) {

        if (newer.major > orignal.major) {
            return 1;
        }

        if (newer.major < orignal.major) {
            return -1;
        }

        if (newer.minor > orignal.minor) {
            return 1;
        }

        if (newer.minor < orignal.minor) {
            return -1;
        }

        if (newer.patch > orignal.patch) {
            return 1;
        }

        if (newer.patch < orignal.patch) {
            return -1;
        }

        return 0;
    }
});