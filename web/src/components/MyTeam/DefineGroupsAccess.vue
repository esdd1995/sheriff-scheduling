<!-- Similar structure to DefineRolesAccess but for Groups -->
<template>
    <b-card bg-variant="white">
        <b-row>
            <b-col cols="11">
                <page-header :pageHeaderText="sectionHeader"></page-header>
            </b-col>
            <b-col style="padding: 0;">
                <b-button v-if="hasPermissionToAddNewGroups" style="max-height: 40px;" size="sm" variant="success" @click="AddGroup()"><b-icon-plus/>Add Group</b-button>
            </b-col>
        </b-row>

        <loading-spinner v-if="!isGroupsDataMounted" />
            
        <b-card v-else bg-variant="light">
            <b-card no-body border-variant="white" bg-variant="white" v-if="!groupData.length">
                <span class="text-muted ml-4 mb-5">No groups.</span>
            </b-card>

            <b-card v-else no-body border-variant="light" bg-variant="white">
                <b-table :items="groupData"
                        :fields="groupFields"
                        class="mx-4"
                        borderless
                        striped
                        small 
                        responsive="sm">
                    <template v-slot:cell(edit)="row">
                        <b-button v-if="hasPermissionToExpireGroups" size="sm" variant="transparent" @click="removeGroup(row.item, row.index)">
                            <b-icon-trash-fill font-scale="1.75" variant="danger"></b-icon-trash-fill>                    
                        </b-button>
                        <b-button size="sm" variant="transparent" @click="openGroupDetails(row.item.id)">
                            <b-icon-pencil-square font-scale="1.75" variant="primary"></b-icon-pencil-square>                    
                        </b-button>
                    </template>
                    <template v-slot:cell(name)="row">                  
                        <span>{{row.item.name}}</span>
                    </template>
                    <template v-slot:cell(description)="row">                  
                        <span>{{ row.item.description}}</span>
                    </template>
                </b-table>
            </b-card>
        </b-card>

        <!-- Group Details Modal -->
        <b-modal size="xl" v-model="showGroupDetails" id="bv-modal-group-details" header-class="bg-primary text-light">            
            <template v-slot:modal-title>                
                <h2 v-if="editMode" class="mb-0 text-light">Updating Group</h2>
                <h2 v-else-if="createMode" class="mb-0 text-light">Creating Group</h2>                
            </template>
            <b-card v-if="isGroupDetailsMounted">
                <b-row class="mx-1"> 
                    <b-form-group class="mr-1" style="width: 20rem">
                        <label>Name<span class="text-danger">*</span></label>
                        <b-form-input v-model="group.name" placeholder="Enter Name" :state="nameState?null:false"></b-form-input>
                    </b-form-group>                                    
                    <b-form-group class="ml-1" style="width: 45rem">
                        <label>Description<span class="text-danger">*</span></label>
                        <b-form-input v-model="group.description" placeholder="Enter Description" :state="descriptionState?null:false"></b-form-input>
                    </b-form-group>
                </b-row>
                <h2 class="mx-1 mt-0"><b-badge v-if="duplicateGroup" variant="danger">Duplicate Group</b-badge></h2>
                <b-row class="mx-1 mt-1">
                    <b-card no-body style="height:300px;width: 66rem;overflow-y: auto; overflow-x:hidden;">
                        <b-form-checkbox-group :state="permissionState?null:false" v-model="selectedPermissions">
                            <label>Permissions<span class="text-danger">*</span></label>
                            <b-row class="mb-2 text-primary" style="font-weight:bold">
                                <b-col cols="4">Name</b-col>
                                <b-col cols="8">Description</b-col>                            
                            </b-row>

                            <b-row v-for="permission in sortPermissions(permissions)" :key="permission.value" class="mb-1">
                                <b-col cols="4">   
                                    <b-form-checkbox                                                                                 
                                        class="mt-1"                                        
                                        @change="permissionChanged" 
                                        :value="permission.value">
                                        {{permission.text}}
                                    </b-form-checkbox>
                                </b-col>
                                <b-col cols="8" style="width: 40rem">
                                    {{permission.desc}}
                                </b-col>
                            </b-row>
                        </b-form-checkbox-group>
                    </b-card>
                </b-row>                
            </b-card>
            <template v-slot:modal-footer>
                <b-button variant="secondary" @click="closeGroupWindow()">
                    <b-icon-x font-scale="1.5" style="padding:0; vertical-align: middle; margin-right: 0.25rem;"></b-icon-x>Cancel
                </b-button>
                <b-button variant="success" :disabled="!hasPermissionToEditGroups && editMode" @click="saveGroup()">
                    <b-icon-check2 style="padding:0; vertical-align: middle; margin-right: 0.25rem;"></b-icon-check2>Save
                </b-button>
            </template>            
        </b-modal>

        <!-- Confirm Delete Modal -->
        <b-modal v-model="confirmDelete" id="bv-modal-confirm-delete" header-class="bg-warning text-light">
            <template v-slot:modal-title>
                <h2 class="mb-0 text-light">Confirm Delete Group</h2>                    
            </template>
            <p>Are you sure you want to delete the "{{groupToDelete.name}}" group?</p>
            <template v-slot:modal-footer>
                <b-button variant="danger" @click="confirmRemoveGroup()">Delete</b-button>
                <b-button variant="primary" @click="confirmDelete = false">Cancel</b-button>
            </template>            
        </b-modal>

        <!-- Cancel Warning Modal -->
        <b-modal v-model="showCancelWarning" id="bv-modal-group-cancel-warning" header-class="bg-warning text-light">            
            <template v-slot:modal-title>                
                <h2 v-if="editMode" class="mb-0 text-light">Unsaved Group Changes</h2>
                <h2 v-else-if="createMode" class="mb-0 text-light">Unsaved New Group</h2>                
            </template>
            <p>Are you sure you want to cancel without saving your changes?</p>
            <template v-slot:modal-footer>
                <b-button variant="secondary" @click="showCancelWarning = false">No</b-button>
                <b-button variant="success" @click="closeWarningWindow()">Yes</b-button>
            </template>            
        </b-modal>

        <!-- Error Modal -->
        <b-modal v-model="openErrorModal" header-class="bg-warning text-light">
            <b-card class="h4 mx-2 py-2">
                <span class="p-0">{{errorText}}</span>
            </b-card>                        
            <template v-slot:modal-footer>
                <b-button variant="primary" @click="openErrorModal=false">Ok</b-button>
            </template>            
        </b-modal> 
    </b-card>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
import { namespace } from 'vuex-class';
import moment from 'moment-timezone';
import * as _ from 'underscore';

import "@store/modules/CommonInformation";
const commonState = namespace("CommonInformation");

import PageHeader from "@components/common/PageHeader.vue";

import { userInfoType } from '@/types/common';
import { groupInfoType, permissionOptionInfoType } from '@/types/MyTeam';
import { groupJsonType } from '@/types/MyTeam/jsonTypes';

@Component({
    components: {
        PageHeader
    }
})
export default class DefineGroupsAccess extends Vue {
    @commonState.State
    public userDetails!: userInfoType;

    showGroupDetails = false;
    showCancelWarning = false;
    group = {} as groupInfoType;
    originalGroup = {} as groupInfoType;
    nameState = true;
    descriptionState = true;
    permissionState = true;
    duplicateGroup = false;        
    isGroupsDataMounted = false;
    isGroupDetailsMounted = false;
    editMode = false;
    createMode = false;
    sectionHeader = "";
    errorText = '';
    hasPermissionToAddNewGroups = false;
    hasPermissionToEditGroups = false;
    hasPermissionToExpireGroups = false;
    selectedPermissions: string[] = [];
    originalSelectedPermissions: string[] = [];
    permissions: permissionOptionInfoType[] = [];

    openErrorModal = false;

    groupFields = [
        { key: 'name', label: 'Name'},
        { key: 'description', label: 'Description'},
        { key: 'edit', thClass: 'd-none'}
    ];

    confirmDelete = false;        
    groupToDelete: groupInfoType = {id:'', name:'', description:'', expiryDate: '', permissions: []};
    indexToDelete = -1;
    groupToEditId = 0;
    
    groupData: groupInfoType[] = [];

    mounted() {
        this.hasPermissionToAddNewGroups = this.userDetails.permissions.includes("CreateAndAssignGroups");
        this.hasPermissionToEditGroups = this.userDetails.permissions.includes("EditGroups");
        this.hasPermissionToExpireGroups = this.userDetails.permissions.includes("ExpireGroups");
        
        this.getGroups();
        this.sectionHeader = "Manage System Groups and Access";
    }

    public getGroups() {
        this.isGroupsDataMounted = false;
        const url = 'api/group';
        this.$http.get(url)
            .then(response => {
                if(response.data) {
                    this.extractGroupsInfo(response.data);                        
                }                    
            }, err => {
                this.errorText = err.response.statusText + ' ' + err.response.status + '  - ' + moment().format();
                if (err.response.status != '401') {
                    this.openErrorModal = true;
                }  
                this.isGroupsDataMounted = true;
            });
    }

    public extractGroupsInfo(data: groupJsonType[]) {
        this.groupData = [];            
        for(const groupInfo of data) {
            const group: groupInfoType = {id:'', name:'', description:'', expiryDate: '', permissions: []};
            group.id = groupInfo.id;
            group.name = groupInfo.name;
            group.description = groupInfo.description;
            group.expiryDate = groupInfo.expiryDate;
            const permissions: string[] = [];               
            for(const permissionInfo of groupInfo.groupPermissions) {                   
                permissions.push(permissionInfo.permissionId);
            }
            group.permissions = permissions;
            this.groupData.push(group);
        }
        this.isGroupsDataMounted = true;
    }

    public openGroupDetails(groupId) {
        this.createMode = false;
        this.editMode = true;
        this.nameState = true;
        this.duplicateGroup = false;
        this.groupToEditId = groupId;
        this.getPermissions();
    }

    public sortPermissions(permissions) {
        return _.sortBy(permissions, 'selected').reverse();        
    }

    public getPermissions() {
        const url = 'api/permission';
        this.$http.get(url)
            .then(response => {
                if(response.data) {
                    this.extractPermissions(response.data);                                               
                }                    
            }, err => {
                this.errorText = err.response.statusText + ' ' + err.response.status + '  - ' + moment().format(); 
                if (err.response.status != '401') {
                    this.openErrorModal = true;
                }  
                this.isGroupsDataMounted = true;
            });
    }

    public closeWarningWindow() {   
        this.resetGroupWindowState();         
        this.showCancelWarning = false;
        this.showGroupDetails = false;
    }

    public loadGroupDetails(groupId) {
        this.editMode = true;            
        const url = 'api/group/' + groupId;
        this.$http.get(url)
            .then(response => {
                if(response.data) {                        
                    this.extractGroupInfo(response.data);                 
                }                    
            }, err => {
                this.errorText = err.response.statusText + ' ' + err.response.status + '  - ' + moment().format(); 
                if (err.response.status != '401') {
                    this.openErrorModal = true;
                } 
                this.isGroupsDataMounted = true;
            });
    }

    public extractGroupInfo(groupData) {
        this.group = {};
        this.selectedPermissions = [];
        this.originalGroup = {};
        this.originalSelectedPermissions = [];
        this.group.id = this.originalGroup.id = groupData.id;
        this.group.name = this.originalGroup.name = groupData.name;
        this.group.description = this.originalGroup.description = groupData.description;            
        for(const groupPermission of groupData.groupPermissions) {
            const index = this.permissions.findIndex(permission => permission.value == groupPermission.permission.id);
            if (index >= 0) {
                this.selectedPermissions.push(this.permissions[index].value);
                this.originalSelectedPermissions.push(this.permissions[index].value);
                this.permissions[index].selected = true;                    
            }
        }
        this.isGroupDetailsMounted = true;
        this.showGroupDetails = true;   
    }

    public permissionChanged() {
        Vue.nextTick().then(() => {
            for(const permissionInx in this.permissions)
                this.permissions[permissionInx].selected = this.selectedPermissions.includes(this.permissions[permissionInx].value);
        });
    }

    public extractPermissions(permissionsData) {
        this.permissions = [];
        this.selectedPermissions = [];
        if (this.createMode) {
            for(const permission of permissionsData) {
                this.permissions.push({text: permission.name, desc: permission.description, value: permission.id, selected: false});
            }
            this.isGroupDetailsMounted = true;
            this.showGroupDetails = true; 
        }
        if (this.editMode) {
            for(const permission of permissionsData) {
                this.permissions.push({text: permission.name, desc: permission.description, value: permission.id, selected: false});
            }
            this.loadGroupDetails(this.groupToEditId);
        }        
    }

    public closeGroupWindow() {                    
        if(this.createMode && this.isEmpty(this.group) && this.selectedPermissions.length < 1) {
            this.showGroupDetails = false;
            this.resetGroupWindowState();
        }             
        else if(this.editMode && !this.changesMade()) {
            this.showGroupDetails = false;
            this.resetGroupWindowState();
        }    
        else
            this.showCancelWarning = true;
    }

    public changesMade(): boolean {            
        return (!_.isEqual(this.originalGroup, this.group) || 
                !_.isEqual(this.originalSelectedPermissions, this.selectedPermissions));
    }

    public isEmpty(obj) {
        for(const prop in obj) 
            if(obj[prop] != null)
                return false;
        return true;
    }        

    public resetGroupWindowState() {
        this.createMode = false;
        this.editMode = false;
        this.nameState = true;
        this.descriptionState = true;
        this.permissionState = true;            
        this.duplicateGroup = false;
        this.group = {} as groupInfoType;
        this.originalGroup = {} as groupInfoType;
        this.selectedPermissions = [];
        this.originalSelectedPermissions = [];
    }

    public AddGroup() {  
        this.createMode = true;
        this.editMode = false;
        this.getPermissions();            
    }

    public saveGroup() { 
        let requiredErrors = 0;
        if (!this.group.name) {
            this.nameState = false;
            requiredErrors += 1;
        } else {
            this.nameState = true;
        }
        if (!this.group.description) {
            this.descriptionState = false;
            requiredErrors += 1;
        } else {
            this.descriptionState = true;
        }
        
        if (!(this.selectedPermissions.length > 0)) {
            this.permissionState = false;
            requiredErrors += 1;
        } else {
            this.permissionState = true;
        }            
        if (requiredErrors == 0) {
            if (this.editMode) this.updateGroup();
            if (this.createMode) this.createGroup();
        }           
    }

    public updateGroup() {
        const body = {
            group: { 
                id: this.group.id,
                name: this.group.name,               
                description: this.group.description
            },
            permissionIds: this.selectedPermissions                
        };

        const url = 'api/group'; 
        this.$http.put(url, body)
            .then(response => {
                if(response.data) {
                    this.resetGroupWindowState();
                    this.showGroupDetails = false;
                    this.getGroups();                     
                }                    
            }, err => {
                this.errorText = err.response.data.error;                    
                if(this.errorText.includes('already exists')) {
                    this.nameState = false;
                    this.duplicateGroup = true;
                }
            });
    }

    public createGroup() {
        const body = {
            group: { 
                name: this.group.name,               
                description: this.group.description
            },
            permissionIds: this.selectedPermissions                
        };

        const url = 'api/group';
        this.$http.post(url, body)
            .then(response => {
                if(response.data) {
                    this.resetGroupWindowState();
                    this.showGroupDetails = false;
                    this.getGroups();                     
                }
            }, err => {
                this.errorText = err.response.data.error;                    
                if(this.errorText.includes('already exists')) {
                    this.nameState = false;
                    this.duplicateGroup = true;
                }
            });   
    }        

    public removeGroup(group: groupInfoType, index: number) {
        this.groupToDelete = group;
        this.indexToDelete = index;            
        this.confirmDelete = true;      
    }

    public confirmRemoveGroup() {
        this.$http.delete('/api/group?id=' + this.groupToDelete.id)
            .then(response => {
                if(response.status == 204) {
                    this.confirmDelete = false;
                    this.getGroups();                                            
                }                    
            });
    }
}
</script>

<style scoped>   
.card {
    border: white;
}
</style> 