<template>
    <b-card no-body>
        <b-tabs pills card>
            <b-tab title="Roles" active>
                <define-roles-access></define-roles-access>
            </b-tab>
            <b-tab title="Groups">
                <define-groups-access></define-groups-access>
            </b-tab>
        </b-tabs>
    </b-card>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import DefineRolesAccess from './DefineRolesAccess.vue';
import DefineGroupsAccess from './DefineGroupsAccess.vue';

@Component({
    components: {
        DefineRolesAccess,
        DefineGroupsAccess
    }
})
export default class ManageAccessControl extends Vue {
}
</script>

<style scoped>
.card {
    border: white;
}
</style> 