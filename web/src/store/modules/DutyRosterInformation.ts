import { assignmentCardInfoType, assignmentCardWeekInfoType, dutyRangeInfoType, myTeamShiftInfoType, selectedDutyCardInfoType} from '@/types/DutyRoster';
import { VuexModule, Module, Mutation, Action } from 'vuex-module-decorators'

@Module({
  namespaced: true
})
class DutyRosterInformation extends VuexModule {

  public dutyRangeInfo = {} as dutyRangeInfoType;
  public shiftAvailabilityInfo: myTeamShiftInfoType[] = [];
  public dutyToBeEdited = '';
  public view24h = false;
  public sheriffFullview = false;
  public printSheriffFullview = false;
  public displayFuelGauge = false;

  public zoomLevel = 100;

  public dutyRosterAssignments: assignmentCardInfoType[] = [];
  public dutyRosterAssignmentsWeek: assignmentCardWeekInfoType[] = [];

  public selectedDuties: selectedDutyCardInfoType[] = [];

  public activeTab: 'Day' | 'Week' = 'Day';
  
  @Mutation
  public setDutyRangeInfo(dutyRangeInfo): void {   
    this.dutyRangeInfo = dutyRangeInfo
  }

  @Action
  public UpdateDutyRangeInfo(newDutyRangeInfo): void {
    this.context.commit('setDutyRangeInfo', newDutyRangeInfo)
  }

  @Mutation
  public setActiveTab(activeTab): void {   
    this.activeTab = activeTab
  }

  @Action
  public UpdateActiveTab(activeTab): void {
    this.context.commit('setActiveTab', activeTab)
  }

  @Mutation
  public setShiftAvailabilityInfo(shiftAvailabilityInfo): void {   
    this.shiftAvailabilityInfo = shiftAvailabilityInfo;
  }

  @Action
  public UpdateShiftAvailabilityInfo(newShiftAvailabilityInfo): void {
    this.context.commit('setShiftAvailabilityInfo', newShiftAvailabilityInfo)
  }

  @Mutation
  public setDutyToBeEdited(dutyToBeEdited): void {   
    this.dutyToBeEdited = dutyToBeEdited;
  }

  @Action
  public UpdateDutyToBeEdited(newDutyToBeEdited): void {
    this.context.commit('setDutyToBeEdited', newDutyToBeEdited)
  }

  @Mutation
  public setView24h(view24h: boolean): void {   
    this.view24h = view24h;
  }

  @Action
  public UpdateView24h(newView24h: boolean): void {
    this.context.commit('setView24h', newView24h)
  }

  @Mutation
  public setSheriffFullview(sheriffFullview: boolean): void {   
    this.sheriffFullview = sheriffFullview;
  }
  @Action
  public UpdateSheriffFullview(newSheriffFullview: boolean): void {
    this.context.commit('setSheriffFullview', newSheriffFullview)
  }

  @Mutation
  public setDisplayFuelGauge(displayFuelGauge: boolean): void {   
    this.displayFuelGauge = displayFuelGauge;
  }
  @Action
  public UpdateDisplayFuelGauge(newDisplayFuelGauge: boolean): void {
    this.context.commit('setDisplayFuelGauge', newDisplayFuelGauge)
  }

  @Mutation
  public setPrintSheriffFullview(printSheriffFullview: boolean): void {   
    this.printSheriffFullview = printSheriffFullview;
  }
  @Action
  public UpdatePrintSheriffFullview(newPrintSheriffFullview: boolean): void {
    this.context.commit('setPrintSheriffFullview', newPrintSheriffFullview)
  }

  @Mutation
  public setZoomLevel(zoomLevel: number): void {   
    this.zoomLevel = zoomLevel;
  }
  @Action
  public UpdateZoomLevel(newZoomLevel: number): void {
    this.context.commit('setZoomLevel', newZoomLevel)
  }

  
  @Mutation
  public setSelectedDuties(selectedDuties): void {   
    this.selectedDuties = selectedDuties
  }

  @Action
  public UpdateSelectedDuties(newSelectedDuties): void {
    this.context.commit('setSelectedDuties', newSelectedDuties)
  }


  @Mutation
  public setDutyRosterAssignments(dutyRosterAssignments: assignmentCardInfoType[]): void {   
    this.dutyRosterAssignments = dutyRosterAssignments
  }

  @Action
  public UpdateDutyRosterAssignments(newDutyRosterAssignments: assignmentCardInfoType[]): void {
    this.context.commit('setDutyRosterAssignments', newDutyRosterAssignments)
  }
  
  
  @Mutation
  public setDutyRosterAssignmentsWeek(dutyRosterAssignmentsWeek: assignmentCardWeekInfoType[]): void {   
    this.dutyRosterAssignmentsWeek = dutyRosterAssignmentsWeek
  }

  @Action
  public UpdateDutyRosterAssignmentsWeek(newDutyRosterAssignmentsWeek: assignmentCardWeekInfoType[]): void {
    this.context.commit('setDutyRosterAssignmentsWeek', newDutyRosterAssignmentsWeek)
  }
  
}

export default DutyRosterInformation