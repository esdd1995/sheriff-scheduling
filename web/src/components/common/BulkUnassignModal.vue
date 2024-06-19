<template>
    <div>
        <b-modal v-model="showModal" id="bv-modal-bulk-unassign" size="lg bulk-unassign" no-close-on-backdrop centered header-class="bg-primary pt-3 pb-2 text-light">            			
            <template v-slot:modal-header-close>                 
                <b-button style="margin:-2.95rem -0.75rem -2rem 1rem; width:2.5rem; height:2.5rem;" variant="outline-warning" class="text-light" @click="closeBulkUnassignModal">
                    <div style="transform:translate(0px,1px)">&times;</div>
                </b-button>
            </template>

            <template v-slot:modal-title>
                <div class="h2 mb-2 text-light"> Bulk Unassign: </div>                 
            </template>

            <loading-spinner v-if="loadingData" />
            
            <h1> Duty Slots </h1>

            <b-row v-if="showBulkUnassignError" id="BulkUnassignError" class="h4 mx-2">
                <b-badge class="mx-1 mt-2"
                    style="width: 20rem;"
                    v-b-tooltip.hover
                    :title="showBulkUnassignErrorMsg"
                    variant="danger"> 
                    
                    {{showBulkUnassignErrorMsg | truncate(40)}}
                    
                    <b-icon class="ml-3"
                        icon = x-square-fill
                        @click="showBulkUnassignError = false"
                    />
                </b-badge>                    
            </b-row>
        
            <b-table
                :items="dutyBlocksToUnassign"
                :fields="fields"
                head-row-variant="primary"
                striped
                borderless
                small
                sort-by="startTimeString"
                responsive="sm"
            >   
                <template v-slot:cell(sheriffName)="data">
                    {{ data.item.firstName }} {{ data.item.lastName }}
                </template>
                
                <template v-slot:cell(dutyDate)="data">
                    {{ data.value|beautify-date-weekday }}
                </template>
            </b-table> 
            
            <template v-slot:modal-footer>
                <b-button variant="danger" class="mr-auto" @click="confirmBulkUnassign()"> 
                    <b-icon-trash-fill style="padding:0; vertical-align: middle; margin-right: 0.25rem;"></b-icon-trash-fill> 
                    Unassign
                </b-button>
                <b-button variant="primary" @click="closeBulkUnassignModal">Cancel</b-button>
            </template>
        </b-modal>

        <!-- confirm bulk unassign modal -->
        <b-modal v-model="showConfirmBulkUnassignModal" id="bv-modal-confirm-bulk-unassign" header-class="bg-warning text-light">
            <template v-slot:modal-title>
                    <h2 class="mb-0 text-light">Confirm Bulk Unassign Duties</h2>                    
            </template>
            
            <h4 >Are you sure you want to bulk unassign the below list of assignments? Please double check the list of assignments.</h4>
            
            <template v-slot:modal-footer>
                <b-button variant="danger" @click="bulkUnassign()">Confirm</b-button>
                <b-button variant="primary" @click="$bvModal.hide('bv-modal-confirm-bulk-unassign')">Cancel</b-button>
            </template>            
            <template v-slot:modal-header-close>                 
                <b-button variant="outline-warning" class="text-light closeButton" @click="$bvModal.hide('bv-modal-confirm-bulk-unassign')"
                >&times;</b-button>
            </template>
        </b-modal>  
</div>
</template>

<script lang="ts">
    import { Component, Vue, Prop } from 'vue-property-decorator';
    import { assignDutyInfoType, assignmentCardWeekInfoType, attachedDutyInfoType, dutyBlockWeekInfoType, selectedDutyCardInfoType} from '@/types/DutyRoster';

    import * as _ from 'underscore';

    import { namespace } from "vuex-class";
    
    import "@store/modules/DutyRosterInformation";   
    const dutyState = namespace("DutyRosterInformation");

    @Component({
        components: {            
        }        
    })  
    export default class BulkUnassignModal extends Vue {
        @Prop({required: true})
        showModal = false;

        @Prop({required: true})
        dutyRostersJson!: attachedDutyInfoType[];

        @dutyState.State
        public dutyRosterAssignmentsWeek!: assignmentCardWeekInfoType[];

        @dutyState.State
        public selectedDuties!: selectedDutyCardInfoType[];

        loadingData = false;

        showConfirmBulkUnassignModal = false;

        fields = [
            {key:'assignmentName',   label:'Assignment',  sortable:false, tdClass: 'border-top', thClass:'h6 align-middle',},      
            {key:'sheriffName',   label:'Sheriff',  sortable:false, tdClass: 'border-top', thClass:'h6 align-middle',},    
            {key:'dutyDate',   label:'Duty Date',  sortable:false, tdClass: 'border-top', thClass:'h6 align-middle',},
            {key:'startTimeString', label:'Start Time',  sortable:false, tdClass: 'border-top', thClass:'h6 align-middle',},
            {key:'endTimeString',   label:'End Time',  sortable:false, tdClass: 'border-top', thClass:'h6 align-middle',},
        ];

        dutyBlocksToUnassign: Array<dutyBlockWeekInfoType & { assignmentName: string }> = [];

        showBulkUnassignError = false;
        showBulkUnassignErrorMsg = '';
    
        mounted() {
            this.$root.$on('bv::modal::show', (bvEvent, modalId) => {
                
                const constructDutyBlocks: Array<dutyBlockWeekInfoType & { assignmentName: string }> = [];
                this.selectedDuties.forEach((sd) => {
                    sd.dutyBlock?.forEach((db) => {
                        const assignmentId = this.dutyRostersJson.find((dr) => dr.id === db.dutyId)?.assignmentId;
                        const findAssignment = this.dutyRosterAssignmentsWeek.find((aw) => aw.assignmentDetail.id === assignmentId);
                        const assignmentName = findAssignment?.assignmentDetail?.name ? findAssignment.assignmentDetail.name : '';

                        constructDutyBlocks.push({...db, assignmentName: assignmentName});
                    });
                });
                
                this.dutyBlocksToUnassign = constructDutyBlocks;
            });
        }

        confirmBulkUnassign() {
            this.showConfirmBulkUnassignModal = true;
        }

        bulkUnassign() {
            this.showConfirmBulkUnassignModal = false;

            // remove the selected dutyslots from DutyRoster.dutySlots[]
            const dutyIds = _.flatten(this.selectedDuties.map((s) => s.dutyBlock?.map((d) => d.dutyId)))
            const dutySlotIds = _.flatten(this.selectedDuties.map((s) => s.dutyBlock?.map((d) => d.dutySlotId)));
            const updateDutyRosterDto: assignDutyInfoType[] = this.dutyRostersJson.filter((s) => {
                if (dutyIds.includes(s.id)) {
                    s.dutySlots = s.dutySlots.filter((ds) => !dutySlotIds.includes(ds.id));
                    return true;
                }

                return false;
            }); 

            this.showBulkUnassignError = false;
            this.loadingData = true;

            const updateDutyRosterUrl = 'api/dutyroster';
            this.$http.put(updateDutyRosterUrl, updateDutyRosterDto).then(response => {
                if(response.data){
                    this.$emit('close', true);
                }
                
                this.showBulkUnassignError = false;
                this.loadingData = false;
            }, err => {
                this.showBulkUnassignErrorMsg = 'Error Bulk unassigning duty slots';
                if (err.response.data.error) {
                    this.showBulkUnassignErrorMsg = err.response.data.error;
                }
                
                this.showBulkUnassignError = true;
                this.loadingData = false;
            }); 
        }

        closeBulkUnassignModal() {
            this.showBulkUnassignError = false;
            this.showModal = false;
            this.$emit('close', false);
        }
    }
</script>
