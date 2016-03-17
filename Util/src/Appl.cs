﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using hoTools.Utils;
using hoTools.Utils.Parameter;
using hoTools.Utils.ActivityParameter;
//using IFM_Addin;

namespace hoTools.Utils.Appls
{
    public class Appl
    {
        


        public static EA.Element getBehaviorForOperation(EA.Repository Repository, EA.Method method)
        {
            EA.Element returnValue = null;
            string behavior = method.Behavior;
            if (behavior.StartsWith("{") & behavior.EndsWith("}"))
            {
                // get object according to behavior
                EA.Element el = Repository.GetElementByGuid(behavior);
                returnValue = el;
            }
            return returnValue;
        }

        public static void DisplayBehaviorForOperation(EA.Repository Repository, EA.Method method)
        {
            string behavior = method.Behavior;
            if (behavior.StartsWith("{") & behavior.EndsWith("}"))
            {
                // get object according to behavior
                EA.Element el = Repository.GetElementByGuid(behavior);
                // Activity
                if (el == null) { }
                else
                {
                    if (el.Type.Equals("Activity") || el.Type.Equals("Interaction") || el.Type.Equals("StateMachine"))
                    {
                        Util.OpenBehaviorForElement(Repository, el);
                    }
                }
            }
        }
        
        public static bool createInteractionForOperation(EA.Repository rep, EA.Method m)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            EA.Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // create a package with the name of the operation
            var pkgTrg = (EA.Package)pkgSrc.Packages.AddNew(m.Name, "");
            pkgTrg.Update();
            pkgSrc.Packages.Refresh();

            // create Class Sequence Diagram in target package
            var pkgSeqDia = (EA.Diagram)pkgTrg.Diagrams.AddNew("Operation:" + m.Name + " Content", "Sequence");
            pkgSeqDia.Update();
            pkgTrg.Diagrams.Refresh();

            // add frame in Sequence diagram
            var frmObj = (EA.DiagramObject)pkgSeqDia.DiagramObjects.AddNew("l=100;r=400;t=25;b=50", "");
            var frm = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "UMLDiagram");
            frm.Update();
            frmObj.ElementID = frm.ElementID;
            //frmObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            frmObj.Update();
            pkgTrg.Elements.Refresh();
            pkgSeqDia.DiagramObjects.Refresh();


            // create Interaction with the name of the operation
            var seq = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "Interaction");
            seq.Notes = "Generated from Operation:\r\n" + m.Visibility + " " + m.Name + ":" + m.ReturnType + ";\r\nDetails see Operation definition!!";
            seq.Update();
            pkgTrg.Elements.Refresh();

            // create sequence diagram beneath Interaction
            var seqDia = (EA.Diagram)seq.Diagrams.AddNew(m.Name, "Sequence");
            seqDia.Update();
            seq.Diagrams.Refresh();

            // create instance from class beneath Interaction
            var obj = (EA.Element)seq.Elements.AddNew("", "Object");
            seq.Elements.Refresh();
            obj.ClassfierID = elClass.ElementID;
            obj.Update();

            // add node object to Sequence Diagram  
            var node = (EA.DiagramObject)seqDia.DiagramObjects.AddNew("l=100;r=180;t=50;b=70", "");
            node.ElementID = obj.ElementID;
            node.Update();


            // Add Heading to diagram
            var noteObj = (EA.DiagramObject)seqDia.DiagramObjects.AddNew("l=40;r=700;t=10;b=25", "");
            var note = (EA.Element)pkgTrg.Elements.AddNew("Text", "Text");

            note.Notes = m.Visibility + " " + elClass.Name + "_" + m.Name + ":" + m.ReturnType;
            note.Update();
            noteObj.ElementID = note.ElementID;
            noteObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            noteObj.Update();
            pkgTrg.Elements.Refresh();
            seqDia.DiagramObjects.Refresh();


            // Link Operation to activity
            Util.setBehaviorForOperation(rep, m, seq);

            // Set show behavior
            Util.setShowBehaviorInDiagram(rep, m);

            

            Util.setFrameLinksToDiagram(rep, frm, seqDia); // link Overview frame to diagram
            frm.Update();
            //rep.ReloadDiagram(actDia.DiagramID);


            return true;
        }

        //------------------------------------------------------------------------------
        // Create default Elements for Statemachine
        //------------------------------------------------------------------------------
        //
        // init
        // state 'State1'
        // final
        // transition from init to 'State1'

        public static bool createDefaultElementsForStateDiagram(EA.Repository rep, EA.Diagram dia, EA.Element stateChart)
        {

            // check if init and final node are available
            bool init = false;
            bool final = false;
            foreach (EA.Element node in stateChart.Elements)
            {
                init |= node.Type == "StateNode" & node.Subtype == 100;
                final |= node.Type == "StateNode" & node.Subtype == 101;
            }
            EA.Element initNode = null;
            if (!init)
            {
                initNode = (EA.Element)stateChart.Elements.AddNew("", "StateNode");
                initNode.Subtype = 3;
                initNode.ParentID = stateChart.ElementID;
                initNode.Update();
                if (dia != null)
                {
                    Util.addSequenceNumber(rep, dia);
                    var initDiaNode = (EA.DiagramObject)dia.DiagramObjects.AddNew("l=295;r=315;t=125;b=135;", "");
                    initDiaNode.Sequence = 1;
                    initDiaNode.ElementID = initNode.ElementID;
                    initDiaNode.Update();
                    Util.setSequenceNumber(rep, dia, initDiaNode, "1");
                }

            }
            EA.Element finalNode = null;
            // create final node
            if (!final)
            {
                finalNode = (EA.Element)stateChart.Elements.AddNew("", "StateNode");
                finalNode.Subtype = 4;
                finalNode.ParentID = stateChart.ElementID;
                finalNode.Update();
                if (dia != null)
                {
                    Util.addSequenceNumber(rep, dia);
                    var finalDiaNode = (EA.DiagramObject)dia.DiagramObjects.AddNew("l=285;r=305;t=745;b=765;", "");
                    finalDiaNode.Sequence = 1;
                    finalDiaNode.ElementID = finalNode.ElementID;
                    finalDiaNode.Update();
                    Util.setSequenceNumber(rep, dia, finalDiaNode, "1");
                }
            }
            // create state node
            var stateNode = (EA.Element)stateChart.Elements.AddNew("", "State");
            stateNode.Subtype = 0;// state
            stateNode.Name = "State1";
            stateNode.ParentID = stateChart.ElementID;
            stateNode.Update();
            if (dia != null)
            {
                Util.addSequenceNumber(rep, dia);
                string pos = "l=300;r=400;t=-400;b=-470";
                var stateDiaNode = (EA.DiagramObject)dia.DiagramObjects.AddNew(pos, "");
                stateDiaNode.Sequence = 1;
                stateDiaNode.ElementID = stateNode.ElementID;
                stateDiaNode.Update();
                Util.setSequenceNumber(rep, dia, stateDiaNode, "1");

                // draw a transition
                var con = (EA.Connector)finalNode.Connectors.AddNew("", "StateFlow");
                con.SupplierID = stateNode.ElementID;
                con.ClientID = initNode.ElementID;
                con.Update();
                finalNode.Connectors.Refresh();
            }


            stateChart.Elements.Refresh();
            dia.DiagramObjects.Refresh();
            bool error1 = dia.Update();
            rep.ReloadDiagram(dia.DiagramID);

            return true;
        }
        //-----------------------------------------------------------------------------------------
        // Create StateMachine for Operation
        //----------------------------------------------------------------------------------
        public static bool createStateMachineForOperation(EA.Repository rep, EA.Method m)
        {
            // get class
            EA.Element elClass = rep.GetElementByID(m.ParentID);
            EA.Package pkgSrc = rep.GetPackageByID(elClass.PackageID);

            // create a package with the name of the operation
            var pkgTrg = (EA.Package)pkgSrc.Packages.AddNew(m.Name, "");
            pkgTrg.Update();
            pkgSrc.Packages.Refresh();

            // create Class StateMachine Diagram in target package
            var pkgSeqDia = (EA.Diagram)pkgTrg.Diagrams.AddNew("Operation:" + m.Name + " Content", "Statechart");
            pkgSeqDia.Update();
            pkgTrg.Diagrams.Refresh();

            // add frame in StateMachine diagram
            var frmObj = (EA.DiagramObject)pkgSeqDia.DiagramObjects.AddNew("l=100;r=400;t=25;b=50", "");
            var frm = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "UMLDiagram");
            frm.Update();
            frmObj.ElementID = frm.ElementID;
            //frmObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            frmObj.Update();
            pkgTrg.Elements.Refresh();
            pkgSeqDia.DiagramObjects.Refresh();


            // create StateMachine with the name of the operation
            var stateMachine = (EA.Element)pkgTrg.Elements.AddNew(m.Name, "StateMachine");
            stateMachine.Notes = "Generated from Operation:\r\n" + m.Visibility + " " + m.Name + ":" + m.ReturnType + ";\r\nDetails see Operation definition!!";
            stateMachine.Update();
            pkgTrg.Elements.Refresh();

            // create Statechart diagram beneath Statemachine
            var chartDia = (EA.Diagram)stateMachine.Diagrams.AddNew(m.Name, "Statechart");
            chartDia.Update();
            stateMachine.Diagrams.Refresh();

            // put the staemachine on the diagram
            var chartObj = (EA.DiagramObject)chartDia.DiagramObjects.AddNew("l=50;r=600;t=100;b=800", "");
            chartObj.ElementID = stateMachine.ElementID;
            chartObj.Update();
            chartDia.DiagramObjects.Refresh();

            // add default nodes (init/final)
            createDefaultElementsForStateDiagram(rep, chartDia, stateMachine);

            // Add Heading to diagram
            var noteObj = (EA.DiagramObject)chartDia.DiagramObjects.AddNew("l=40;r=700;t=10;b=25", "");
            var note = (EA.Element)pkgTrg.Elements.AddNew("Text", "Text");

            note.Notes = m.Visibility + " " + elClass.Name + "_" + m.Name + ":" + m.ReturnType;
            note.Update();
            noteObj.ElementID = note.ElementID;
            noteObj.Style = "fontsz=200;pitch=34;DUID=265D32D5;font=Arial Narrow;bold=0;italic=0;ul=0;charset=0;";
            noteObj.Update();
            pkgTrg.Elements.Refresh();
            chartDia.DiagramObjects.Refresh();


            // Link Operation to StateMachine
            Util.setBehaviorForOperation(rep, m, stateMachine);

            // Set show behavior
            Util.setShowBehaviorInDiagram(rep, m);



            Util.setFrameLinksToDiagram(rep, frm, chartDia); // link Overview frame to diagram
            frm.Update();
            //rep.ReloadDiagram(actDia.DiagramID);


            return true;
        }
   
    }
}
