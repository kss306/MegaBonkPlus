export const CHEATS_CONFIG = [
    {
        id: 'unlock_all_achievements',
        label: 'Achievements',
        buttonLabel: 'Unlock All',
        type: 'button',
        actionId: 'unlock_all',
        defaultPayload: {},

        confirm: {
            title: 'Unlock All Achievements?',
            message: `
                <p>This action is irreversible.</p>
                <p>It will unlock every achievement in the game <strong>and on Steam</strong>.</p>
                <p>Are you sure you want to continue?</p>
            `,
            confirmLabel: 'Unlock All',
            cancelLabel: 'Cancel',
            variant: 'danger'
        }
    },
    {
        id: 'interact_with_every_charge',
        label: 'All Charge Shrines',
        buttonLabel: 'Interact',
        type: 'button',
        actionId: 'interact_with_every',
        defaultPayload: {
            interactable: 'ChargeShrine'
        },
        confirm: null
    },
    {
        id: 'interact_with_every_moai',
        label: 'All Maoi Shrines',
        buttonLabel: 'Interact',
        type: 'button',
        actionId: 'interact_with_every',
        defaultPayload: {
            interactable: 'MoaiShrine'
        },
        confirm: null
    },
    {
        id: 'interact_with_every_cursed',
        label: 'All Cursed Shrines',
        buttonLabel: 'Interact',
        type: 'button',
        actionId: 'interact_with_every',
        defaultPayload: {
            interactable: 'CursedShrine'
        },
        confirm: null
    },
    {
        id: 'interact_with_every_greed',
        label: 'All Greed Shrines',
        buttonLabel: 'Interact',
        type: 'button',
        actionId: 'interact_with_every',
        defaultPayload: {
            interactable: 'GreedShrine'
        },
        confirm: null
    },
    {
        id: 'interact_with_every_Challenge',
        label: 'All Challenge Shrines',
        buttonLabel: 'Interact',
        type: 'button',
        actionId: 'interact_with_every',
        defaultPayload: {
            interactable: 'ChallengeShrine'
        },
        confirm: null
    },
];