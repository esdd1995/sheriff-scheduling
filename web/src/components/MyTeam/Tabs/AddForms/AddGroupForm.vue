<template>
    <div>
        <b-table-simple small borderless>
            <b-tbody>
                <b-tr>
                    <b-td>   
                        <b-tr class="mt-1 mb-1 pb-0 bg-white">   
                            <h4 class="ml-3 my-1 pb-0">Group: </h4>                          
                        </b-tr>
                        <b-tr>                           
                            <b-form-group v-if="isCreate" style="padding: 0; margin: 0rem 0 0 .5rem;width: 15rem"> 
                                <b-form-select
                                    tabindex="1"
                                    size="sm"
                                    v-model="selectedGroup"
                                    :state="groupState?null:false">
                                        <b-form-select-option :value="{}">
                                            Select a Group*
                                        </b-form-select-option>
                                        <b-form-select-option
                                            v-for="group in groupTypeInfoList" 
                                            :key="group.value"                                            
                                            :value="group">
                                                {{group.text}}
                                        </b-form-select-option>     
                                </b-form-select>                                
                            </b-form-group>
                            <b-form-group v-else style="border-radius: 4px; border:1px solid #bbbbbb; padding:0 0 .0 0;; margin: 0 0 0 0.5rem;width: 15rem">                                
                                <b-form-text class="h5 align-middle my-2 font-weight-normal ml-2">
                                    {{this.selectedGroup.text}}
                                </b-form-text>
                            </b-form-group>
                        </b-tr>                        
                    </b-td>
                    <b-td>
                        <label class="h6 m-0 p-0"> Effective Date: </label>
                        <b-form-datepicker
                            tabindex="2"
                            class="mb-1"
                            size="sm"
                            v-model="selectedEffectiveDate"
                            placeholder="Eff. Date"
                            locale="en-US" 
                            :date-format-options="{ year: 'numeric', month: 'short', day: '2-digit' }"
                            :state="effectiveDateState?null:false">
                        </b-form-datepicker>                       
                    </b-td>
                    <b-td>
                        <label class="h6 m-0 p-0"> Expiry Date: </label>
                        <b-form-datepicker
                            tabindex="3"
                            class="mb-1 mt-0 pt-0"
                            size="sm"
                            v-model="selectedExpiryDate"
                            placeholder="Exp. Date"
                            reset-button
                            locale="en-US"
                            :date-format-options="{ year: 'numeric', month: 'short', day: '2-digit' }">
                        </b-form-datepicker> 
                    </b-td>
                    <b-td>
                        <b-button                                    
                            style="margin: 1.5rem .5rem 0 0 ; padding:0 .5rem 0 .5rem; "
                            variant="secondary"
                            @click="closeForm()">
                            Cancel
                        </b-button>   
                        <b-button                                    
                            style="margin: 1.5rem 0 0 0; padding:0 0.7rem 0 0.7rem; "
                            variant="success"                                                    
                            @click="saveForm()">
                            Save
                        </b-button>  
                    </b-td>
                </b-tr>   
            </b-tbody>
        </b-table-simple>  

        <b-modal v-model="showCancelWarning" id="bv-modal-group-cancel-warning" header-class="bg-warning text-light">            
            <template v-slot:modal-title>
                <h2 v-if="isCreate" class="mb-0 text-light"> Unsaved New Group </h2>                
                <h2 v-else class="mb-0 text-light"> Unsaved Group Changes </h2>                                 
            </template>
            <p>Are you sure you want to cancel without saving your changes?</p>
            <template v-slot:modal-footer>
                <b-button variant="secondary" @click="$bvModal.hide('bv-modal-group-cancel-warning')"                   
                >No</b-button>
                <b-button variant="success" @click="confirmedCloseForm()"
                >Yes</b-button>
            </template>            
            <template v-slot:modal-header-close>                 
                 <b-button variant="outline-warning" class="text-light closeButton" @click="$bvModal.hide('bv-modal-group-cancel-warning')"
                 >&times;</b-button>
            </template>
        </b-modal>             
    </div>
</template>

<script lang="ts">
    import { Component, Vue, Prop } from 'vue-property-decorator';
    import { namespace } from 'vuex-class';

    import {teamMemberInfoType, groupOptionInfoType, userGroupInfoType} from '@/types/MyTeam';
    
    import "@store/modules/TeamMemberInformation"; 
    const TeamMemberState = namespace("TeamMemberInformation");

    @Component
    export default class AddGroupForm extends Vue {        

        @TeamMemberState.State
        public userToEdit!: teamMemberInfoType;

        @Prop({required: true})
        formData!: userGroupInfoType;

        @Prop({required: true})
        isCreate!: boolean;

        @Prop({required: true})
        groupTypeInfoList!: groupOptionInfoType[];       

        selectedGroup = {} as groupOptionInfoType | undefined;
        groupState = true;      

        selectedExpiryDate = '';
        expiryDateState = true; 

        selectedEffectiveDate = '';
        effectiveDateState = true;

        originalEffectiveDate = '';
        originalExpiryDate = '';

        formDataId = '';
        showCancelWarning = false;
        
        mounted() { 
            this.clearSelections();            
            if(this.formData.value) {
                this.extractFormInfo();
            }               
        }        

        public extractFormInfo() {
            this.formDataId = this.formData.value? this.formData.value:'';
            this.selectedGroup = {text: this.formData.text, desc: this.formData.desc, value: this.formData.value};               
            this.originalEffectiveDate = this.selectedEffectiveDate = this.formData.effectiveDate.substring(0,10)            
            this.originalExpiryDate = this.selectedExpiryDate = this.formData.expiryDate? this.formData.expiryDate.substring(0,10): '';
        }

        public saveForm() {
            this.groupState = true;
            this.effectiveDateState = true;                

            if(this.selectedGroup && !this.selectedGroup.value) {
                this.groupState = false;
            }
            else if(this.selectedEffectiveDate == "") {
                this.groupState = true;
                this.effectiveDateState = false;
            }
            else {
                this.groupState = true;
                this.effectiveDateState = true;

                const timezone = this.userToEdit.homeLocation? this.userToEdit.homeLocation.timezone :'UTC';
                const effectiveDate = Vue.filter('convertDate')(this.selectedEffectiveDate,"", 'StartTime',timezone);
                const expiryDate = this.selectedExpiryDate? Vue.filter('convertDate')(this.selectedExpiryDate,"",'EndTime',timezone): '';

                const body = [{                        
                    effectiveDate: effectiveDate,
                    expiryDate: expiryDate,
                    groupId: this.selectedGroup?this.selectedGroup.value:'',
                    text: this.selectedGroup?this.selectedGroup.text:'',
                    desc: this.selectedGroup?this.selectedGroup.desc:''
                }] 
                this.$emit('submit', body, this.isCreate);                  
            }
        }

        public closeForm() {
            if(this.isChanged())
                this.showCancelWarning = true;
            else
                this.confirmedCloseForm();
        }

        public isChanged() {
            if(this.isCreate) {
                if((this.selectedGroup && this.selectedGroup.value) ||
                    this.selectedEffectiveDate || this.selectedExpiryDate) return true;
                return false;
            } else {
                if((this.originalEffectiveDate != this.selectedEffectiveDate)|| 
                    (this.originalExpiryDate != this.selectedExpiryDate)) return true;
                return false;
            }
        }

        public confirmedCloseForm() {           
            this.clearSelections();
            this.$emit('cancel');
        }

        public clearSelections() {
            this.selectedGroup = {} as groupOptionInfoType;
            this.selectedEffectiveDate = '';
            this.selectedExpiryDate = '';
            this.groupState = true;
            this.effectiveDateState = true;
            this.expiryDateState = true;           
        }       
    }
</script>

<style scoped>
    td {
        margin: 0rem 0.5rem 0.1rem 0rem;
        padding: 0rem 0.5rem 0.1rem 0rem;
        background-color: white;
    }
</style> 