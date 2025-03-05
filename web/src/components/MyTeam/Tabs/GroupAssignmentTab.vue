<template>
    <div>
        <b-card v-if="groupTabDataReady"  style="height:400px;overflow: auto;" >
            <b-card id="GroupError" no-body>
                <h2 v-if="groupError" class="mx-1 mt-2"><b-badge v-b-tooltip.hover :title="groupErrorMsgDesc"  variant="danger"> {{groupErrorMsg}} <b-icon class="ml-3" icon = x-square-fill @click="groupError = false" /></b-badge></h2>
            </b-card>
            <b-card  v-if="!addNewGroupForm && hasPermissionToAssignGroups && hasPermissionToEditUsers">                
                <b-button v-if="allowChanges" size="sm" variant="success" :disabled="groups.length==0" @click="addNewGroup"> <b-icon icon="plus" /> Add </b-button>
            </b-card>

            <b-card v-if="addNewGroupForm" id="addGroupForm" class="my-3" :border-variant="addFormColor" style="border:2px solid" body-class="m-0 px-0 py-1">
                <add-group-form :formData="{}" :isCreate="true" :groupTypeInfoList="groups" v-on:submit="saveGroup" v-on:cancel="closeGroupForm" />              
            </b-card>

            <div>
                <b-card no-body border-variant="white" bg-variant="white" v-if="!assignedGroups.length">
                    <span class="text-muted ml-4 mb-5">No groups have been assigned.</span>
                </b-card>

                <b-card v-else no-body border-variant="light" bg-variant="white">
                    <b-table
                        :items="filteredAssignedGroups"
                        :fields="groupFields"
                        :key="refreshTable"
                        head-row-variant="primary"
                        striped
                        borderless
                        small
                        sort-by="effectiveDate"
                        responsive="sm"
                        >
                            <template v-slot:head(editGroup)>
                                <b-form-group style="margin: 0; padding: 0; width: 7.30rem;"> 
                                    <b-form-select
                                        size = "sm"
                                        v-model="selectedGroupView">
                                            <b-form-select-option value="active">
                                                Active Groups
                                            </b-form-select-option>
                                            <b-form-select-option value="history">
                                                History
                                            </b-form-select-option>     
                                    </b-form-select>
                                </b-form-group>
                            </template>  

                            <template v-slot:cell(effectiveDate)="data" >
                                <span>{{data.value | beautify-date}}</span> 
                            </template>
                            <template v-slot:cell(expiryDate)="data" >
                                <span>{{data.value | beautify-date}}</span> 
                            </template>
                            <template v-slot:cell(editGroup)="data" >                                       
                                <span><b-button class="m-0"
                                                style="padding: 1px 2px 1px 2px;" 
                                                size="sm"
                                                v-b-tooltip.hover
                                                title="Expire" 
                                                variant="warning"
                                                v-if="hasPermissionToEditUsers && allowChanges" 
                                                @click="confirmDeleteGroup(data.item)">
                                                <b-icon icon="clock" 
                                                        font-scale="1.25" 
                                                        variant="white"/>
                                </b-button></span>
                                <span><b-button class="my-0 py-0" 
                                                size="sm" 
                                                variant="transparent"
                                                v-if="hasPermissionToEditUsers && allowChanges" 
                                                @click="editGroup(data)">
                                                <b-icon icon="pencil-square" 
                                                        font-scale="1.25" 
                                                        variant="primary"/>
                                </b-button></span> 
                            </template>

                            <template v-slot:row-details="data">
                                <b-card :id="'Gr-Date-'+data.item.effectiveDate.substring(0,10)" :border-variant="addFormColor" body-class="m-0 px-0 py-1" style="border:2px solid">
                                    <add-group-form :formData="data.item" :isCreate="false" :groupTypeInfoList="groups" v-on:submit="saveGroup" v-on:cancel="closeGroupForm" />
                                </b-card>
                            </template>                            
                    </b-table> 
                </b-card>
            </div>                                        
        </b-card>

         <b-modal v-model="confirmDelete" id="bv-modal-confirm-delete" header-class="bg-warning text-light">
            <template v-slot:modal-title>
                    <h2 class="mb-0 text-light">Confirm Expire Group</h2>                    
            </template>            
            <h4>Are you sure you want to expire the "{{groupToDelete.desc}}" group?</h4>
            <b-form-group style="margin: 0; padding: 0; width: 20rem;"><label class="ml-1">Reason for Expiration:</label> 
                <b-form-select
                    size = "sm"
                    v-model="groupDeleteReason">
                        <b-form-select-option value="OPERDEMAND">
                            Cover Operational Demands
                        </b-form-select-option>
                        <b-form-select-option value="PERSONAL">
                            Personal Decision
                        </b-form-select-option>
                        <b-form-select-option value="ENTRYERR">
                            Entry Error
                        </b-form-select-option>     
                </b-form-select>
            </b-form-group>
            <template v-slot:modal-footer>
                <b-button variant="danger" @click="deleteGroup()" :disabled="groupDeleteReason.length == 0">Confirm</b-button>
                <b-button variant="primary" @click="cancelDeletion()">Cancel</b-button>
            </template>            
            <template v-slot:modal-header-close>                 
                <b-button variant="outline-warning" class="text-light closeButton" @click="cancelDeletion()"
                >&times;</b-button>
            </template>
        </b-modal> 

        <b-modal v-model="openErrorModal" header-class="bg-warning text-light">
            <b-card class="h4 mx-2 py-2">
				<span class="p-0">{{errorText}}</span>
            </b-card>                        
            <template v-slot:modal-footer>
                <b-button variant="primary" @click="openErrorModal=false">Ok</b-button>
            </template>            
            <template v-slot:modal-header-close>                 
                <b-button variant="outline-warning" class="text-light closeButton" @click="openErrorModal=false"
                >&times;</b-button>
            </template>
        </b-modal>    
    </div>
</template>

<script lang="ts">
    import { Component, Vue } from 'vue-property-decorator';
    import moment from 'moment-timezone';    
    import { namespace } from 'vuex-class';

    import "@store/modules/CommonInformation";
    const commonState = namespace("CommonInformation"); 

    import "@store/modules/TeamMemberInformation";
    const TeamMemberState = namespace("TeamMemberInformation");

    import AddGroupForm from './AddForms/AddGroupForm.vue';
    import { userGroupHistoryJsonType, userGroupJsonType } from '@/types/MyTeam/jsonTypes';
    import { locationInfoType, userInfoType } from '@/types/common';
    import { groupOptionInfoType, teamMemberInfoType, userGroupInfoType } from '@/types/MyTeam';

    @Component({
        components: {
            AddGroupForm
        }        
    }) 
    export default class GroupAssignmentTab extends Vue {

        @TeamMemberState.State
        public userToEdit!: teamMemberInfoType;
        
        @commonState.State
        public location!: locationInfoType;

        @commonState.State
        public userDetails!: userInfoType;

        hasPermissionToEditUsers = false;
        hasPermissionToAssignGroups = false;
        groupTabDataReady = false;       

        refreshTable = 0;

        groups: groupOptionInfoType[] = []
        groupAssignError = false;

        groupsJson;
        historicGroupsJson;
        addNewGroupForm = false;
        addFormColor = 'secondary';
        latestEditData;
        isEditOpen = false;
        allowChanges = true;

        groupError = false;
        groupErrorMsg = '';
        groupErrorMsgDesc = '';
        updateTable=0;

        confirmDelete = false;
        groupToDelete = {} as userGroupInfoType;
        assignedGroups: userGroupInfoType[] = [];
        historicAssignedGroups: userGroupInfoType[] = [];
        groupDeleteReason = '';
        selectedGroupView = 'active';

        errorText = ''
        openErrorModal=false;

        timezone = 'UTC';

        groupFields =  
        [           
            {key:'text',    label:'Group',sortable:false, tdClass: 'border-top',  }, 
            {key:'effectiveDate', label:'Effective Date',   sortable:false, tdClass: 'border-top', thClass:'',},
            {key:'expiryDate', label:'Expiry Date',      sortable:false, tdClass: 'border-top', thClass:'',}, 
            {key:'editGroup', label:'', sortable:false, tdClass: 'border-top', thClass:'text-white',},       
        ];

        mounted()
        {
            this.hasPermissionToEditUsers = this.userDetails.permissions.includes("EditUsers");                         
            this.hasPermissionToAssignGroups = this.userDetails.permissions.includes("CreateAndAssignGroups");
            this.timezone = this.userToEdit.homeLocation? this.userToEdit.homeLocation.timezone :'UTC';
            this.groupTabDataReady = false;
            this.allowChanges = true;
            this.loadGroups();
        }
   
        public loadGroups(){
            const url = '/api/group';
            this.$http.get(url)
                .then(response => {
                    if(response.data){
                        this.groupsJson = response.data
                        this.extractGroups();                        
                    }                                   
                },err => {
                    this.errorText=err.response.statusText+' '+err.response.status + '  - ' + moment().format(); 
                    if (err.response.status != '401') {
                        this.openErrorModal=true;
                    }  
                })
        }
      
        public extractGroups(){
            this.groups=[];
            this.assignedGroups =[];
            this.groupAssignError = false;

            const url = `api/sheriff/${this.userToEdit.id}/groups`;

            this.$http.get(url).then((response) => {
                if (response.data && response.data.length>0) {
                    let userGroup: userGroupJsonType;
                    for(userGroup of response.data) 
                    {                    
                        this.assignedGroups.push({
                            text:userGroup.group.name, 
                            desc: userGroup.group.description, 
                            value:userGroup.group.id.toString(), 
                            effectiveDate:moment(userGroup.effectiveDate).tz(this.timezone).format(), 
                            expiryDate:userGroup.expiryDate?moment(userGroup.expiryDate).tz(this.timezone).format():''
                        })
                    }
                }
                
                this.refreshTable++;
                this.populateGroupsDropdown();
            });
        }

        public populateGroupsDropdown() {
            this.groups=[];
            const today = moment().tz(this.location.timezone);
            
            for(const group of this.groupsJson)
            {                
                const index = this.assignedGroups.findIndex(assigngroup =>{if(assigngroup.value == group.id) return true;else return false});
                if(index < 0)
                {             
                    this.groups.push({text:group.name, desc: group.description, value:group.id})           
                } else {
                    const expiryDate = this.assignedGroups[index].expiryDate;
                     
                    if (expiryDate.length && today.isAfter(moment(expiryDate).tz(this.location.timezone))) {
                        this.groups.push({text:group.name, desc: group.description, value:group.id})
                    }                   
                }
            }
            this.loadHistoricGroups();            
        }

        public loadHistoricGroups(){
            const url = '/api/audit/grouphistory?sheriffId='+ this.userToEdit.id;
            this.$http.get(url)
                .then(response => {
                    if(response.data){
                        this.historicGroupsJson = response.data
                        this.extractHistoricGroups();                        
                    }                                   
                },err => {
                    this.errorText=err.response.statusText+' '+err.response.status + '  - ' + moment().format(); 
                    if (err.response.status != '401') {
                        this.openErrorModal=true;
                    }    
                })
        }

        public extractHistoricGroups(){
           
            this.historicAssignedGroups = [];

            if (this.historicGroupsJson && this.historicGroupsJson.length>0) {
                let userGroup: userGroupHistoryJsonType;
                for(userGroup of this.historicGroupsJson) 
                {
                    if (userGroup.oldValuesJson && userGroup.oldValuesJson.ExpiryDate && userGroup.keyValuesJson && userGroup.keyValuesJson.GroupId) {
                        const groupId = userGroup.keyValuesJson.GroupId;
                        const group = this.groupsJson.filter(group =>{if(group.id == groupId ) return true})[0]                  
                        this.historicAssignedGroups.push({
                            text:group.name, 
                            desc: group.description, 
                            value:groupId.toString(), 
                            effectiveDate:userGroup.oldValuesJson.EffectiveDate? moment(userGroup.oldValuesJson.effectiveDate).tz(this.timezone).format():'', 
                            expiryDate:userGroup.oldValuesJson.ExpiryDate?moment(userGroup.oldValuesJson.expiryDate).tz(this.timezone).format():''
                        })
                    }                     
                }
            }            
            this.groupTabDataReady = true;
        }

        get filteredAssignedGroups(){
            if (this.selectedGroupView == 'active') {
                this.allowChanges = true;
                return this.assignedGroups;
            } else {
                this.allowChanges = false;
                return this.historicAssignedGroups;
            }
        }
        
        public addNewGroup(){
            if(this.isEditOpen){
                location.href = '#Gr-Date-'+this.latestEditData.item.effectiveDate.substring(0,10)
                this.addFormColor = 'danger'
            }else{
                this.addNewGroupForm = true;
                this.$nextTick(()=>{location.href = '#addGroupForm';})
            }
        }

        public saveGroup(body, iscreate){                            
            this.groupError = false; 
            body[0]['userId']= this.userToEdit.id;
            const method = 'put';   
            const url = 'api/sheriff/assigngroups';
            const options = { method: method, url:url, data:body} 
            this.$http(options)
                .then(response => {
                    if(iscreate) 
                        this.addToGroupList(body[0]);
                    else
                        this.modifyAssignedGroupList(body[0]);
                    this.closeGroupForm();
                    this.populateGroupsDropdown()
                }, err=>{
                    const errMsg = err.response.data.error;
                    this.groupErrorMsg = errMsg.slice(0,60) + (errMsg.length>60?' ...':'');
                    this.groupErrorMsgDesc = errMsg;
                    this.groupError = true;
                    location.href = '#GroupError'
                });           
        }

        public modifyAssignedGroupList(modifiedGroupInfo){
            const index = this.assignedGroups.findIndex(assignedgroup =>{ if(assignedgroup.value == modifiedGroupInfo.groupId) return true})
            if(index>=0){
                this.assignedGroups[index].value =  modifiedGroupInfo.groupId;
                this.assignedGroups[index].effectiveDate = moment(modifiedGroupInfo.effectiveDate).tz(this.timezone).format();
                this.assignedGroups[index].expiryDate = modifiedGroupInfo.expiryDate? moment(modifiedGroupInfo.expiryDate).tz(this.timezone).format():'';
                this.assignedGroups[index].text = modifiedGroupInfo.text;
                this.assignedGroups[index].desc = modifiedGroupInfo.desc;                  
                this.$emit('change');
            } 
        }

        public addToGroupList(addedGroupInfo){
            const group = {} as userGroupInfoType;
            group.value = addedGroupInfo.groupId;
            group.text = addedGroupInfo.text;
            group.desc = addedGroupInfo.desc; 
            group.effectiveDate = moment(addedGroupInfo.effectiveDate).tz(this.timezone).format();
            group.expiryDate = addedGroupInfo.expiryDate? moment(addedGroupInfo.expiryDate).tz(this.timezone).format():'';
            this.assignedGroups.push(group); 
            this.$emit('change');                     
        }

        public closeGroupForm() {                     
            this.addNewGroupForm= false; 
            this.addFormColor = 'secondary'
            if(this.isEditOpen){
                this.latestEditData.toggleDetails();
                this.isEditOpen = false;
            } 
        }

        public confirmDeleteGroup(group) {
            this.groupToDelete = group;           
            this.confirmDelete = true; 
        }

        public cancelDeletion() {
            this.confirmDelete = false;
            this.groupDeleteReason = '';
        }

        public deleteGroup(){
            if (this.groupDeleteReason.length) {
                this.groupAssignError = false;
                this.confirmDelete = false;                 
                const body = 
                [{
                    "userId": this.userToEdit.id,
                    "groupId": this.groupToDelete.value,
                    "expiryReason": this.groupDeleteReason                        
                }]
                const url = 'api/sheriff/unassigngroups' ;
                this.$http.put(url, body)
                    .then(response => {
                        this.updateDeletedGroup();
                        this.$emit('change');
                                                        
                    }, err=>{
                        const errMsg = err.response.data.error;
                        this.groupErrorMsg = errMsg.slice(0,60) + (errMsg.length>60?' ...':'');
                        this.groupErrorMsgDesc = errMsg;                    
                        this.groupAssignError = true;
                    });
                this.groupDeleteReason = '';
            }
        }
        
        public updateDeletedGroup(){ 
            const index = this.assignedGroups.findIndex(assignedgroup =>{ if(assignedgroup.value == this.groupToDelete.value) return true})
            if(index>=0){
                this.assignedGroups[index].expiryDate = new Date().toISOString();            
                this.$emit('change');
            } 
        }
        
        public editGroup(data){
            if(this.addNewGroupForm){
                location.href = '#addGroupForm'
                this.addFormColor = 'danger'
            }else if(this.isEditOpen){
                location.href = '#Gr-Date-'+this.latestEditData.item.effectiveDate.substring(0,10)
                this.addFormColor = 'danger'               
            }else if(!this.isEditOpen && !data.detailsShowing){
                data.toggleDetails();
                this.isEditOpen = true;
                this.latestEditData = data
                Vue.nextTick().then(()=>{
                    location.href = '#Gr-Date-'+this.latestEditData.item.effectiveDate.substring(0,10)
                });
            }
        }
    }
</script>

<style scoped>
    .card {
        border: white;
    }
</style> 