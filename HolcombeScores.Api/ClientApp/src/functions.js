class Functions {
    static except(selected, remove) {
        let copy = Object.assign({}, selected);
        delete copy[remove];
        return copy;
    }

    static union(selected, add) {
        let copy = Object.assign({}, selected);
        copy[add] = true;
        return copy;
    }

    static toUtcDateTime(date) {
        const pad = (num) => {
            return num.toString().padStart(2, '0');
        }

        const dateStr = `${date.getUTCFullYear()}-${pad(date.getUTCMonth() + 1)}-${pad(date.getUTCDate())}`;
        const timeStr = `${pad(date.getUTCHours())}:${pad(date.getUTCMinutes())}:00.000Z`;
        return `${dateStr}T${timeStr}`;
    }

    static toLocalDateTime(date) {
        const pad = (num) => {
            return num.toString().padStart(2, '0');
        }

        const dateStr = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
        const timeStr = `${pad(date.getHours())}:${pad(date.getMinutes())}`;
        return `${dateStr}T${timeStr}`;
    }

    static playerSortFunction(playerA, playerB) {
        if (playerA.name.toLowerCase() === playerB.name.toLowerCase()) {
            return 0;
        }

        return (playerA.name.toLowerCase() > playerB.name.toLowerCase())
            ? 1
            : -1;
    }

    static playerAvailabilitySortFunction(availabilityA, availabilityB) {
        return Functions.playerSortFunction(availabilityA.player, availabilityB.player);
    }

    static gameSortFunction(gameA, gameB) {
        // sort games descending by date
        const aDate = new Date(gameA.date);
        const bDate = new Date(gameB.date);

        return bDate.getTime() - aDate.getTime();
    }

    static teamSortFunction(teamA, teamB) {
        if (teamA.name.toLowerCase() === teamB.name.toLowerCase()) {
            return 0;
        }

        return (teamA.name.toLowerCase() > teamB.name.toLowerCase())
            ? 1
            : -1;
    }

    static recoverySortFunction(recoveryA, recoveryB) {
        if (recoveryA.name.toLowerCase() === recoveryB.name.toLowerCase()) {
            return 0;
        }

        return (recoveryA.name.toLowerCase() > recoveryB.name.toLowerCase())
            ? 1
            : -1;
    }

    static getResultMessages(result) {
        let messages = [];
        if (result.messages) {
            result.messages.forEach(m => messages.push(m));
        }
        if (result.warnings) {
            result.warnings.forEach(m => messages.push('Warning: ' + m));
        }
        if (result.errors) {
            if (result.errors.length) {
                // is an array
                result.errors.forEach(m => messages.push('Error: ' + m));
            } else {
                // is an object
                Object.keys(result.errors).forEach(key => {
                    messages.push(`${key}: ${result.errors[key]}`);
                });
            }
        }
        if (result.title) {
            messages.push(result.title);
        }

        return messages.join('\n');
    }

    static getSharingLink() {
        return document.location.href;
    }
}

export { Functions };
